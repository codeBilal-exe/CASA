# MISS - Mini Internet Simulation System

A comprehensive educational project that simulates a miniature internet ecosystem using **C networking servers** and a **C# WinForms browser application**.

## 🎯 Project Overview

MISS is an **educational simulation** designed to demonstrate the core mechanics of the internet:

- **DNS Resolution**: How domain names (like `apple.com`) are converted to IP addresses via UDP.
- **Web Hosting**: How web servers listen for TCP connections and serve HTML content.
- **Client-Server Lifecycle**: How to manage and monitor distributed server processes.
- **Network Failure Handling**: Real-world scenarios like server timeouts and offline states.

## 📁 Project Structure

```
MISS/
├── Browser/                      # C# WinForms Client
│   ├── Forms/                    
│   │   └── MainBrowserForm.cs    # Web Browser + Server Settings UI
│   ├── Services/                 
│   │   ├── NetworkingService.cs  # DNS (UDP) & HTTP (TCP) logic
│   │   └── ServerControlService.cs # Process management (Start/Stop)
│   └── Models/                   
│       └── PacketLog.cs          # Data model for logging
│
├── Servers/
│   └── DNS_Server/               # UDP DNS Server (C)
│       ├── DNS_Server.c
│       └── DNS_Server.exe        # Resolves apple.com, google.com, github.com
│
├── Websites/                     # Individual Website Servers (C)
│   ├── apple/                    # Port 8081
│   │   ├── apple_server.c
│   │   ├── apple_server.exe
│   │   └── index.html
│   ├── google/                   # Port 8082
│   │   ├── google_server.c
│   │   ├── google_server.exe
│   │   └── index.html
│   └── github/                   # Port 8083
│       ├── github_server.c
│       ├── github_server.exe
│       └── index.html
│
└── compile_servers.bat           # One-click recompile for all C servers
```

## 🚀 Quick Start

### 1. Prerequisites
- **GCC (MinGW)** installed and in your system PATH.
- **.NET 8.0 SDK** (for the C# Browser).

### 2. Compilation
If you modify any server code, run the compilation script from the project root:
```powershell
./compile_servers.bat
```

### 3. Running the System
1. Launch the **MISS Browser** application.
2. Navigate to the **Server Settings** tab.
3. Start the **DNS Server** and your desired **Website Servers**.
4. Go back to the **Web Browser** tab and enter a domain (e.g., `apple.com`).

## 💻 System Components

### DNS Server (UDP Port 5053)
- Handles incoming domain requests.
- Responds with `IP:PORT` or `NOT_FOUND`.
- Hardcoded records for `apple.com`, `google.com`, and `github.com`.

### Website Servers (TCP Ports 8081-8083)
- Each website runs its own dedicated C server process.
- Listens for `GET / HTTP/1.1` requests.
- Streams the local `index.html` file back to the browser.

### Browser (C# WinForms)
- **Networking**: Uses `UdpClient` for DNS and `TcpClient` for web pages.
- **Server Control**: Uses the `Process` class to start/stop the C executables.
- **Logging**: Real-time visualization of network packets (UDP/TCP flow).
- **Error Handling**: Graceful handling of timeouts, offline servers, and invalid domains.

## 🔄 System Flow

1. **User Input**: Enter `apple.com` in the address bar.
2. **DNS Query (UDP)**: Browser asks DNS Server (Port 5053) for the address.
3. **DNS Response**: DNS returns `127.0.0.1:8081`.
4. **TCP Connection**: Browser connects to the Apple Server on port 8081.
5. **HTTP Request**: Browser sends `GET /`.
6. **Server Response**: Apple Server reads `index.html` and sends HTML data.
7. **Rendering**: Browser displays the "Think different" page.

## 🎓 Educational Objectives
- Understand **Socket Programming** in C and C#.
- Learn the difference between **UDP** (connectionless) and **TCP** (connection-oriented).
- Observe **Process Management** and how applications interact with external services.
- Debug common network errors in a simulated environment.

---
**MISS: Where Learning Meets Networking** 🌐
