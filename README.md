# CASA - Client And Server Architecture

A premium educational simulation that demonstrates the core mechanics of the internet through a modern **Blazor Hybrid** interface and distributed **C-based networking servers**.

## 🎯 Project Overview

CASA (Client And Server Architecture) is a full-stack educational tool designed to peel back the layers of how the web works. It simulates the entire lifecycle of a web request:

- **DNS Resolution**: Convert domain names to IP addresses via low-level UDP queries.
- **Distributed Web Hosting**: Multiple independent C-based servers simulating global websites.
- **Process Orchestration**: A central control panel to manage the lifecycle of simulated server infrastructure.
- **Live Packet Capture**: Real-time visualization of UDP (DNS) and TCP (HTTP) traffic flows.
- **Robust Error Simulation**: Hands-on experience with timeouts, offline states, and NXDOMAIN errors.

## 📁 Project Architecture

```text
CASA-System/
├── CASA-Client/                  # Modern Blazor UI Client
│   ├── Components/               # Razor Components (Pages, Layouts, UI)
│   ├── Services/                 # Core Logic (Networking, Server Control)
│   └── Models/                   # Data structures for logs and info
│
├── Servers/
│   └── DNS_Server/               # UDP DNS Resolver (C)
│       └── DNS_Server.c          # Robust resolver logic
│
├── Websites/                     # Independent Website Nodes (C)
│   ├── apple/                    # Port 8081 - Simulation of Apple.com
│   ├── google/                   # Port 8082 - Simulation of Google.com
│   └── github/                   # Port 8083 - Simulation of GitHub.com
│
├── compile_servers.bat           # Automated compilation for all C servers
└── run.bat                       # Master launcher for the entire system
```

## 🚀 Getting Started

### 1. Prerequisites

- **GCC (MinGW)**: Required to compile the C-based networking servers.
- **.NET 8.0 SDK**: Required to run the modern Blazor-based browser.

### 2. Compilation

To ensure all server binaries are up to date with the latest networking logic:

```powershell
./compile_servers.bat
```

### 3. Launching the System

Simply run the master launcher script:

```powershell
./run.bat
```

## 💻 System Components

### 🌐 The Browser (Blazor Hybrid)

A high-performance, reactive UI that acts as the client.

- **Networking Engine**: Implements raw `UdpClient` for DNS and `TcpClient` for HTTP/1.0.
- **Control Center**: Directly manages external server processes through the `System.Diagnostics.Process` API.
- **Live Terminal**: A beautiful live log viewer that captures every packet exchanged in the system.

### 🔍 DNS Resolver (UDP 5053)

The heart of the simulation's routing logic.

- Listens for raw UDP packets containing domain names.
- Uses a hash-map style lookup to resolve simulated domains.
- Provides real-time feedback in its own console window.

### 📄 Website Servers (TCP 8081-8083)

Atomic, lightweight C servers representing independent web nodes.

- Each server is a separate process, simulating a distributed internet.
- Handles standard HTTP/1.1 `GET` requests and serves local `index.html` files.

## 🔄 The "Internet" Flow

1. **User Request**: User enters `apple.com` in the browser.
2. **DNS Phase (UDP)**: Browser sends a UDP datagram to `127.0.0.1:5053`.
3. **DNS Reply**: Server responds with `127.0.0.1:8081`.
4. **HTTP Phase (TCP)**: Browser establishes a TCP handshake with the Apple node.
5. **Data Exchange**: Browser sends a `GET` request; server streams HTML content.
6. **Rendering**: The browser's sandboxed viewport renders the received content.

---

**CASA: Engineering the Internet, One Packet at a Time.** 🌐
