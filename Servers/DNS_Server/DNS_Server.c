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

DNSEntry dns_table[] = {{"apple.com", "127.0.0.1:8080"},
                        {"google.com", "127.0.0.1:8080"},
                        {"github.com", "127.0.0.1:8080"},
                        {"youtube.com", "127.0.0.1:8080"}};

int main() {
  WSADATA wsa;
  SOCKET s;
  struct sockaddr_in server, client;
  int c, recv_len;
  char buf[BUF_SIZE];

  printf("==========================================\n");
  printf("        CASA DNS RESOLVER SERVER          \n");
  printf("==========================================\n");

  if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
    printf("Failed to initialize Winsock. Error Code : %d\n",
           WSAGetLastError());
    system("pause");
    return 1;
  }

  if ((s = socket(AF_INET, SOCK_DGRAM, 0)) == INVALID_SOCKET) {
    printf("Could not create socket : %d\n", WSAGetLastError());
    system("pause");
    return 1;
  }

  server.sin_family = AF_INET;
  server.sin_addr.s_addr = INADDR_ANY;
  server.sin_port = htons(PORT);

  if (bind(s, (struct sockaddr *)&server, sizeof(server)) == SOCKET_ERROR) {
    int err = WSAGetLastError();
    if (err == 10048) {
      printf("ERROR: Port %d is already in use!\n", PORT);
      printf("Please close any existing DNS_Server.exe processes.\n");
    } else {
      printf("Bind failed with error code : %d\n", err);
    }
    closesocket(s);
    WSACleanup();
    system("pause");
    return 1;
  }

  printf("DNS Server listening on UDP port %d...\n", PORT);
  printf("Ready to resolve: apple.com, google.com, github.com, youtube.com\n\n");

  while (1) {
    memset(buf, 0, BUF_SIZE);
    c = sizeof(struct sockaddr_in);

    recv_len =
        recvfrom(s, buf, BUF_SIZE - 1, 0, (struct sockaddr *)&client, &c);
    if (recv_len == SOCKET_ERROR) {
      printf("recvfrom() failed with error code : %d\n", WSAGetLastError());
      break;
    }

    buf[recv_len] = '\0'; // Ensure null termination

    printf("[QUERY] Domain: %s\n", buf);
    fflush(stdout);

    char response[50] = "NOT_FOUND";
    for (int i = 0; i < 4; i++) {
      if (_stricmp(buf, dns_table[i].domain) == 0) {
        strcpy(response, dns_table[i].ip_port);
        break;
      }
    }

    printf("[REPLY] -> %s\n", response);
    fflush(stdout);

    if (sendto(s, response, (int)strlen(response), 0,
               (struct sockaddr *)&client, c) == SOCKET_ERROR) {
      printf("sendto() failed with error code : %d\n", WSAGetLastError());
      // Don't break, try to keep serving
    }
  }

  closesocket(s);
  WSACleanup();

  return 0;
}
