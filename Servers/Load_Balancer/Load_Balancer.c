#include <stdio.h>
#include <string.h>
#include <winsock2.h>
#include <windows.h>
#include <stdlib.h>
#include <time.h>

#pragma comment(lib, "ws2_32.lib")

#define BUF_SIZE 8192

typedef struct {
    char domain[100];
    int port;
    int weight;
    int active_connections;
} ServerConfig;

ServerConfig servers[100];
int server_count = 0;
char algorithm[50] = "Round Robin";

int rr_index = 0;
int wrr_index = 0;
int wrr_current_weight = 0;
char last_config_snapshot[4096] = {0};

void reload_config() {
    FILE *f = fopen("lb_config.json", "r");
    if (!f) return;
    
    char json[4096] = {0};
    fread(json, 1, sizeof(json)-1, f);
    fclose(f);
    
    char *algo_ptr = strstr(json, "\"Algorithm\":");
    if (algo_ptr) {
        algo_ptr += 12;
        while (*algo_ptr == ' ' || *algo_ptr == '"') algo_ptr++;
        int i = 0;
        while (*algo_ptr != '"' && *algo_ptr != ',' && *algo_ptr != '\r' && *algo_ptr != '\n' && i < 49) {
            algorithm[i++] = *algo_ptr++;
        }
        algorithm[i] = '\0';
    }
    
    // Naive parse for servers array
    int new_server_count = 0;
    ServerConfig new_servers[100];
    
    char *ptr = json;
    while ((ptr = strstr(ptr, "\"domain\":")) != NULL) {
        ptr += 9;
        while (*ptr == ' ' || *ptr == '"') ptr++;
        int i = 0;
        while (*ptr != '"' && i < 99) {
            new_servers[new_server_count].domain[i++] = *ptr++;
        }
        new_servers[new_server_count].domain[i] = '\0';
        
        ptr = strstr(ptr, "\"port\":");
        if (ptr) {
            ptr += 7;
            new_servers[new_server_count].port = atoi(ptr);
        }
        
        ptr = strstr(ptr, "\"weight\":");
        if (ptr) {
            ptr += 9;
            new_servers[new_server_count].weight = atoi(ptr);
        } else {
            new_servers[new_server_count].weight = 1;
        }
        new_servers[new_server_count].active_connections = 0;
        
        new_server_count++;
    }
    
    // Copy active connections from old config if ports match
    for (int i = 0; i < new_server_count; i++) {
        for (int j = 0; j < server_count; j++) {
            if (new_servers[i].port == servers[j].port && strcmp(new_servers[i].domain, servers[j].domain) == 0) {
                new_servers[i].active_connections = servers[j].active_connections;
                break;
            }
        }
    }
    
    memcpy(servers, new_servers, sizeof(ServerConfig) * new_server_count);
    server_count = new_server_count;

    if (strcmp(last_config_snapshot, json) != 0) {
        strncpy(last_config_snapshot, json, sizeof(last_config_snapshot) - 1);
        last_config_snapshot[sizeof(last_config_snapshot) - 1] = '\0';

        /* Reset weighted counter when config changes */
        wrr_index = 0;
        wrr_current_weight = 0;
    }
}

void write_stats(const char* last_route) {
    FILE *f = fopen("lb_stats.json", "w");
    if (!f) return;
    fprintf(f, "{\n  \"last_route\": \"%s\",\n  \"connections\": {\n", last_route ? last_route : "");
    for (int i = 0; i < server_count; i++) {
        fprintf(f, "    \"%d\": %d%s\n", servers[i].port, servers[i].active_connections, i == server_count - 1 ? "" : ",");
    }
    fprintf(f, "  }\n}\n");
    fclose(f);
}

// Thread to proxy data
DWORD WINAPI handle_client(LPVOID arg) {
    SOCKET client_socket = (SOCKET)arg;
    char buffer[BUF_SIZE];
    
    // Reload config on every request (simple enough for low traffic)
    reload_config();
    
    // Peek at HTTP request
    int bytes_received = recv(client_socket, buffer, BUF_SIZE - 1, 0);
    if (bytes_received <= 0) {
        closesocket(client_socket);
        return 0;
    }
    buffer[bytes_received] = '\0';
    
    // Extract Host header
    char host[100] = {0};
    char *host_header = strstr(buffer, "Host: ");
    if (host_header) {
        host_header += 6;
        int i = 0;
        while (*host_header != '\r' && *host_header != '\n' && *host_header != ':' && i < 99) {
            host[i++] = *host_header++;
        }
        host[i] = '\0';
    }
    
    // Select backend
    int selected_idx = -1;
    
    if (strcmp(algorithm, "Least Connections") == 0) {
        int min_conn = 999999;
        for (int i = 0; i < server_count; i++) {
            if (strcmp(servers[i].domain, host) == 0) {
                if (servers[i].active_connections < min_conn) {
                    min_conn = servers[i].active_connections;
                    selected_idx = i;
                }
            }
        }
    } else if (strcmp(algorithm, "Weighted Round Robin") == 0) {
        /*
         * Deterministic Weighted Round Robin:
         * wrr_index = current server position in the ordered list
         * wrr_current_weight = how many times current server
         *                      has been served in this cycle
         *
         * Algorithm: serve server[wrr_index] for weight[wrr_index]
         * times, then move to next server, repeat.
         * This guarantees exact weight distribution.
         */

        /* Build candidate list for this domain */
        int candidates[100];
        int cand_count = 0;
        for (int i = 0; i < server_count; i++) {
            if (strcmp(servers[i].domain, host) == 0 && servers[i].weight > 0) {
                candidates[cand_count++] = i;
            }
        }

        /* Serve heavier servers first within each deterministic cycle. */
        for (int i = 0; i < cand_count - 1; i++) {
            for (int j = i + 1; j < cand_count; j++) {
                if (servers[candidates[j]].weight > servers[candidates[i]].weight) {
                    int tmp = candidates[i];
                    candidates[i] = candidates[j];
                    candidates[j] = tmp;
                }
            }
        }

        if (cand_count > 0) {
            /* Wrap wrr_index to valid range for this domain */
            wrr_index = wrr_index % cand_count;

            int idx = candidates[wrr_index];

            /* Serve this server one more time */
            wrr_current_weight++;

            /* If served enough times for its weight, move to next */
            if (wrr_current_weight >= servers[idx].weight) {
                wrr_current_weight = 0;
                wrr_index = (wrr_index + 1) % cand_count;
            }

            selected_idx = idx;
        }
    } else {
        // Round Robin
        int count = 0;
        int candidates[100];
        for (int i = 0; i < server_count; i++) {
            if (strcmp(servers[i].domain, host) == 0) {
                candidates[count++] = i;
            }
        }
        if (count > 0) {
            selected_idx = candidates[rr_index % count];
            rr_index++;
        }
    }
    
    if (selected_idx == -1) {
        printf("[TRACE] Unknown host: %s\n", host);
        closesocket(client_socket);
        return 0;
    }
    
    InterlockedIncrement((LONG*)&servers[selected_idx].active_connections);
    
    char route_msg[200];
    sprintf(route_msg, "%s -> %d", host, servers[selected_idx].port);
    write_stats(route_msg);
    
    printf("[TRACE] Host: %s -> Routed to Port: %d (Algo: %s, Conns: %d)\n", 
           host, servers[selected_idx].port, algorithm, servers[selected_idx].active_connections);
    fflush(stdout);
    
    // Connect to backend
    SOCKET backend_socket = socket(AF_INET, SOCK_STREAM, 0);
    struct sockaddr_in backend_addr;
    backend_addr.sin_family = AF_INET;
    backend_addr.sin_addr.s_addr = inet_addr("127.0.0.1");
    backend_addr.sin_port = htons(servers[selected_idx].port);
    
    if (connect(backend_socket, (struct sockaddr*)&backend_addr, sizeof(backend_addr)) < 0) {
        printf("[ERROR] Failed to connect to backend port %d\n", servers[selected_idx].port);
        InterlockedDecrement((LONG*)&servers[selected_idx].active_connections);
        write_stats("connection failed");
        closesocket(client_socket);
        closesocket(backend_socket);
        return 0;
    }
    
    // Forward the initial request
    send(backend_socket, buffer, bytes_received, 0);
    
    // Forward response back
    while ((bytes_received = recv(backend_socket, buffer, BUF_SIZE, 0)) > 0) {
        send(client_socket, buffer, bytes_received, 0);
    }
    
    InterlockedDecrement((LONG*)&servers[selected_idx].active_connections);
    write_stats(route_msg);
    
    closesocket(backend_socket);
    closesocket(client_socket);
    return 0;
}

int main() {
    WSADATA wsa;
    SOCKET s;
    struct sockaddr_in server;
    
    printf("==========================================\n");
    printf("        CASA LOAD BALANCER SERVER         \n");
    printf("==========================================\n");
    
    if (WSAStartup(MAKEWORD(2,2), &wsa) != 0) return 1;
    if ((s = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET) return 1;
    
    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;
    server.sin_port = htons(8080);
    
    if (bind(s, (struct sockaddr *)&server, sizeof(server)) == SOCKET_ERROR) return 1;
    
    listen(s, 100);
    srand((unsigned int)time(NULL));
    printf("Load Balancer listening on port 8080...\n\n");
    
    while(1) {
        SOCKET new_socket;
        struct sockaddr_in client;
        int c = sizeof(struct sockaddr_in);
        new_socket = accept(s, (struct sockaddr *)&client, &c);
        if (new_socket != INVALID_SOCKET) {
            CreateThread(NULL, 0, handle_client, (LPVOID)new_socket, 0, NULL);
        }
    }
    
    closesocket(s);
    WSACleanup();
    return 0;
}
