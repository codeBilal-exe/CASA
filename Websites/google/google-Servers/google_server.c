#include <stdio.h>
#include <string.h>
#include <winsock2.h>
#include <stdlib.h>

#pragma comment(lib, "ws2_32.lib")

#define BUF_SIZE 4096

void send_response(SOCKET client_socket, const char *file_path) {
    char header[] = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n";
    send(client_socket, header, strlen(header), 0);

    FILE *f = fopen(file_path, "r");
    if (f == NULL) {
        char error[] = "<html><body><h1>404 Not Found</h1></body></html>";
        send(client_socket, error, strlen(error), 0);
        return;
    }

    char buf[BUF_SIZE];
    int bytes_read;
    while ((bytes_read = fread(buf, 1, BUF_SIZE, f)) > 0) {
        send(client_socket, buf, bytes_read, 0);
    }
    fclose(f);
}

int main() {
    int port = 8082;
    const char *site_path = "../index.html";

    WSADATA wsa;
    SOCKET s, new_socket;
    struct sockaddr_in server, client;
    int c;

    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) return 1;
    if ((s = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET) return 1;

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;
    server.sin_port = htons(port);

    if (bind(s, (struct sockaddr *)&server, sizeof(server)) == SOCKET_ERROR) return 1;

    listen(s, 3);
    printf("Google Server started on port %d...\n", port);
    fflush(stdout);

    while (1) {
        c = sizeof(struct sockaddr_in);
        new_socket = accept(s, (struct sockaddr *)&client, &c);
        if (new_socket == INVALID_SOCKET) break;

        char buffer[BUF_SIZE];
        recv(new_socket, buffer, BUF_SIZE, 0);
        send_response(new_socket, site_path);
        closesocket(new_socket);
    }

    closesocket(s);
    WSACleanup();
    return 0;
}
