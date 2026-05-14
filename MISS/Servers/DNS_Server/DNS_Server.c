#include <stdio.h>
#include <string.h>
#include <winsock2.h>

#pragma comment(lib, "ws2_32.lib")

#define PORT 5053
#define BUF_SIZE 1024

typedef struct {
    char domain[100];
    char ip_port[50];
} DNSEntry;

DNSEntry dns_table[] = {
    {"apple.com", "127.0.0.1:8081"},
    {"google.com", "127.0.0.1:8082"},
    {"github.com", "127.0.0.1:8083"}
};

int main() {
    WSADATA wsa;
    SOCKET s;
    struct sockaddr_in server, client;
    int c, recv_len;
    char buf[BUF_SIZE];

    printf("Initializing DNS Server...\n");
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
        printf("Failed. Error Code : %d", WSAGetLastError());
        return 1;
    }

    if ((s = socket(AF_INET, SOCK_DGRAM, 0)) == INVALID_SOCKET) {
        printf("Could not create socket : %d", WSAGetLastError());
    }

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;
    server.sin_port = htons(PORT);

    if (bind(s, (struct sockaddr *)&server, sizeof(server)) == SOCKET_ERROR) {
        printf("Bind failed with error code : %d", WSAGetLastError());
        return 1;
    }

    printf("DNS Server listening on UDP port %d...\n", PORT);

    while (1) {
        fflush(stdout);
        memset(buf, 0, BUF_SIZE);

        c = sizeof(struct sockaddr_in);
        if ((recv_len = recvfrom(s, buf, BUF_SIZE, 0, (struct sockaddr *)&client, &c)) == SOCKET_ERROR) {
            printf("recvfrom() failed with error code : %d", WSAGetLastError());
            break;
        }

        printf("Received DNS request for: %s\n", buf);
        fflush(stdout);

        char response[50] = "NOT_FOUND";
        for (int i = 0; i < 3; i++) {
            if (strcmp(buf, dns_table[i].domain) == 0) {
                strcpy(response, dns_table[i].ip_port);
                break;
            }
        }

        printf("Sending response: %s\n", response);
        fflush(stdout);
        if (sendto(s, response, strlen(response), 0, (struct sockaddr *)&client, c) == SOCKET_ERROR) {
            printf("sendto() failed with error code : %d", WSAGetLastError());
            break;
        }
    }

    closesocket(s);
    WSACleanup();

    return 0;
}
