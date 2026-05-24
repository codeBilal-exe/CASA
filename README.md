# CASA — Client And Server Architecture

> An educational simulation of socket programming that demonstrates the core mechanics of the internet through a modern **Blazor Hybrid** interface and distributed **C-based networking servers**.

---

## What is CASA?

CASA is a full-stack educational tool that simulates the complete lifecycle of a web request — from typing a domain name to receiving an HTML page — using real socket programming. Every component in the chain (DNS, routing, load balancing, HTTP servers) is implemented as an actual running process communicating over TCP/UDP sockets on localhost.

The project is designed for computer networks students who want to see how the internet works at the code level, not just in diagrams.

---

## Project Architecture

```
CASA/
├── CASA-Client/                        # Blazor Hybrid Desktop UI
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Browser.razor           # Simulated browser with address bar
│   │   │   ├── Servers.razor           # Server process control panel
│   │   │   ├── LoadBalancer.razor      # Load balancer config & monitoring
│   │   │   └── Cache.razor             # DNS/HTTP cache viewer
│   │   ├── Layout/
│   │   │   └── MainLayout.razor        # App shell and navigation
│   │   ├── PacketLogViewer.razor       # Live packet capture terminal
│   │   └── ServerCard.razor            # Individual server status card
│   ├── Services/
│   │   ├── NetworkingService.cs        # UDP (DNS) + TCP (HTTP) client logic
│   │   ├── ServerControlService.cs     # Launch/stop server processes
│   │   ├── PacketLogService.cs         # Real-time packet event bus
│   │   └── CacheService.cs             # In-memory DNS and HTTP cache
│   └── Models/
│       ├── PacketLog.cs
│       ├── ServerInfo.cs
│       └── CacheEntry.cs
│
├── Servers/
│   ├── DNS_Server/
│   │   └── DNS_Server.c                # UDP DNS resolver (port 5053)
│   └── Load_Balancer/
│       ├── Load_Balancer.c             # TCP load balancer (port 8080)
│       ├── lb_config.json              # Algorithm + server pool config
│       └── lb_stats.json               # Runtime connection stats
│
├── Websites/
│   ├── Youtube/
│   │   ├── index.html                  # YouTube UI clone
│   │   └── yoututbe-Servers/
│   │       └── youtube_server.c        # TCP server — port 8087
│   ├── apple/
│   │   ├── index.html                  # Apple.com UI clone
│   │   └── apple_Servers/
│   │       ├── apple_server.c          # TCP server — port 8081
│   │       ├── apple2_server.c         # TCP server — port 8084
│   │       └── apple3_server.c         # TCP server — port 8085
│   ├── google/
│   │   ├── index.html                  # Google.com UI clone
│   │   └── google-Servers/
│   │       ├── google_server.c         # TCP server — port 8082
│   │       └── google2_server.c        # TCP server — port 8086
│   └── github/
│       ├── index.html                  # GitHub.com UI clone
│       └── github_server.c             # TCP server — port 8083
│
├── compile_servers.bat                 # Compile all C servers with GCC
└── run.bat                             # Full system launcher
```

---

## Simulated Domains & Ports

| Domain | Primary Port | Additional Ports | Notes |
|---|---|---|---|
| `youtube.com` | 8087 | — | Single server |
| `apple.com` | 8081 | 8084, 8085 | 3 servers behind load balancer |
| `google.com` | 8082 | 8086 | 2 servers behind load balancer |
| `github.com` | 8083 | — | Single server |
| DNS Resolver | UDP 5053 | — | Resolves all above domains |
| Load Balancer | TCP 8080 | — | Routes based on lb_config.json |

---

## Getting Started

### Prerequisites

| Requirement | Purpose | Download |
|---|---|---|
| **.NET 8.0 SDK** | Run the Blazor desktop client | https://dotnet.microsoft.com/download |
| **GCC (MinGW)** | Compile C networking servers | https://mingw-w64.org |

### Quick Start

**1. Clone the repository**
```bash
git clone https://github.com/codeBilal-exe/CASA.git
cd CASA
```

**2. Run the launcher**

The `run.bat` script handles everything: it checks prerequisites, compiles all C servers if binaries are missing, restores NuGet packages, builds the .NET project, and launches the application.

```bat
./run.bat
```

**3. Manual compilation (optional)**

If you want to recompile the C servers independently:
```bat
./Servers/DNS_Server/compile_servers.bat
```

---

## How a Request Works

```
User types "youtube.com" in the Browser page
        |
        v
[DNS Phase — UDP]
NetworkingService sends UDP datagram to 127.0.0.1:5053
DNS_Server.c looks up "youtube.com" → returns "127.0.0.1:8080"
        |
        v
[Load Balancer — TCP]
Client connects to 127.0.0.1:8080
Load_Balancer.c reads lb_config.json, selects youtube server (port 8087)
        |
        v
[HTTP Phase — TCP]
Client connects to 127.0.0.1:8087
youtube_server.c receives GET request, reads index.html, streams it back
        |
        v
[Rendering]
Blazor Browser page renders the received HTML in a sandboxed viewport
PacketLogService logs every step in the Live Terminal
```

---

## System Components

### Blazor Hybrid Client

The desktop application built with .NET Blazor Hybrid (.NET 8 + MAUI). It runs as a native Windows app while rendering Razor components in an embedded WebView.

**Pages:**

- **Browser** — Address bar, navigation controls, rendered page viewport. Runs the full DNS + HTTP flow on each request.
- **Servers** — Start/stop each C server process individually. Shows live status (running/stopped), PID, and port for each node.
- **Load Balancer** — Configure the balancing algorithm (Round Robin, Least Connections, Weighted Round Robin). Displays real-time connection counts per backend.
- **Cache** — View DNS resolution cache and HTTP response cache entries with TTL and hit/miss counters.

**Key Services:**

- `NetworkingService` — Raw `UdpClient` for DNS queries, `TcpClient` for HTTP/1.0 requests. Implements 2-second DNS timeout and 3-second HTTP connection timeout.
- `ServerControlService` — Wraps `System.Diagnostics.Process` to spawn and terminate server `.exe` files. Tracks process state.
- `PacketLogService` — Event-driven log bus. Every DNS query, TCP connection, and HTTP exchange emits a timestamped packet event visible in the Live Terminal.

### DNS Server (`DNS_Server.c`)

A UDP server listening on port 5053. Maintains a static lookup table mapping simulated domain names to `ip:port` strings. Responds with `NOT_FOUND` for unknown domains.

- Protocol: UDP
- Port: 5053
- Responds within a single `recvfrom`/`sendto` cycle

### Load Balancer (`Load_Balancer.c`)

A TCP server that reads `lb_config.json` on startup (and can reload at runtime). For each incoming client connection it selects a backend server according to the configured algorithm and proxies the full TCP stream.

**Supported algorithms:**
- Round Robin
- Least Connections
- Weighted Round Robin

Configuration is live-editable through the Load Balancer page in the UI — changes are written to `lb_config.json` and picked up by the server.

### Website Servers

Each website is a self-contained C TCP server that:

1. Binds to a specific port
2. Accepts a TCP connection
3. Reads the HTTP GET request from the socket buffer
4. Opens `index.html` from disk and streams it as an HTTP/1.1 response
5. Closes the connection

Each server runs as a separate OS process, simulating a distributed hosting environment. The Apple and Google sites have multiple server instances to demonstrate load balancing.

---

## Load Balancer Configuration

`Servers/Load_Balancer/lb_config.json` controls the full server pool:

```json
{
  "Algorithm": "Round Robin",
  "Servers": [
    { "domain": "youtube.com", "port": 8087, "weight": 1 },
    { "domain": "apple.com",   "port": 8081, "weight": 8 },
    { "domain": "apple.com",   "port": 8084, "weight": 2 },
    { "domain": "apple.com",   "port": 8085, "weight": 9 },
    { "domain": "google.com",  "port": 8082, "weight": 1 },
    { "domain": "google.com",  "port": 8086, "weight": 1 },
    { "domain": "github.com",  "port": 8083, "weight": 1 }
  ]
}
```

Change `"Algorithm"` to `"Least Connections"` or `"Weighted Round Robin"` and save — the Load Balancer page will pick up the change.

---

## Troubleshooting

**Port already in use**

The DNS server will print `ERROR: Port 5053 is already in use!` if a previous instance is still running. Open Task Manager, find `DNS_Server.exe`, and end the process. Same applies to any other server port.

**Binaries not found**

Run `compile_servers.bat` manually. Make sure `gcc` is in your system PATH. To verify: `gcc --version` in a terminal.

**Blazor app fails to start**

Ensure .NET 8.0 SDK is installed: `dotnet --version`. If the version shown is below 8.0, download the correct SDK.

**Page loads blank**

The target server may not be running. Go to the Servers page, confirm the relevant server shows a green "Running" status, and retry the request.

---

## Technologies

| Layer | Technology |
|---|---|
| Desktop UI | .NET 8 Blazor Hybrid |
| UI Framework | Razor Components |
| Networking Client | `System.Net.Sockets` (UdpClient, TcpClient) |
| Process Control | `System.Diagnostics.Process` |
| Web Servers | C (Winsock2, TCP) |
| DNS Resolver | C (Winsock2, UDP) |
| Load Balancer | C (Winsock2, TCP proxy) |
| Config Format | JSON |
| Build Tools | GCC (MinGW), .NET CLI |

---

## Learning Outcomes

By running and exploring CASA you will observe:

- How a domain name query travels over UDP and returns an IP:port pair
- How a TCP connection is established between a client and a server
- How raw HTTP GET requests are formed and how servers parse and respond to them
- How a load balancer sits between a client and a pool of servers
- How multiple server processes independently serve identical content
- How Round Robin and Weighted algorithms distribute connections differently
- How packet-level events can be logged and visualized in real time

---

**CASA — Engineering the Internet, One Packet at a Time.**