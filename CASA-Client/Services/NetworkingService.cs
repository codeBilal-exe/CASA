using System.Net.Sockets;
using System.Text;

namespace CASA_Client.Services;

public class NetworkingService
{
    private const string DNS_IP = "127.0.0.1";
    private const int DNS_PORT = 5053;

    // ── DNS (UDP) ────────────────────────────────────────────────────────────
    public async Task<(string ip, int port)> ResolveDnsAsync(string domain)
    {
        try
        {
            using var udp = new UdpClient();
            udp.Client.ReceiveTimeout = 2000;
            udp.Connect(DNS_IP, DNS_PORT);

            byte[] req = Encoding.ASCII.GetBytes(domain);
            await udp.SendAsync(req, req.Length);

            var recv = udp.ReceiveAsync();
            if (await Task.WhenAny(recv, Task.Delay(2000)) != recv)
                throw new Exception("DNS_TIMEOUT");

            string response = Encoding.ASCII.GetString((await recv).Buffer);
            if (response == "NOT_FOUND") return ("NOT_FOUND", 0);

            var parts = response.Split(':');
            return (parts[0], int.Parse(parts[1]));
        }
        catch (SocketException)
        {
            throw new Exception("DNS_OFFLINE");
        }
    }

    // ── HTTP (TCP) ───────────────────────────────────────────────────────────
    public async Task<string> FetchPageAsync(string ip, int port)
    {
        try
        {
            using var tcp = new TcpClient();
            var conn = tcp.ConnectAsync(ip, port);
            if (await Task.WhenAny(conn, Task.Delay(3000)) != conn)
                throw new Exception("SERVER_TIMEOUT");
            await conn;

            using var stream = tcp.GetStream();
            stream.ReadTimeout = 5000;

            // HTTP/1.0 so server closes after response — no chunked encoding
            string req = "GET / HTTP/1.0\r\nHost: localhost\r\nConnection: close\r\n\r\n";
            byte[] reqBytes = Encoding.ASCII.GetBytes(req);
            await stream.WriteAsync(reqBytes);

            // Read entire response
            using var ms = new MemoryStream();
            byte[] buf = new byte[4096];
            int read;
            while ((read = await stream.ReadAsync(buf)) > 0)
                ms.Write(buf, 0, read);

            string full = Encoding.UTF8.GetString(ms.ToArray());

            // Strip HTTP headers
            int sep = full.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            return sep >= 0 ? full[(sep + 4)..] : full;
        }
        catch (SocketException)
        {
            throw new Exception("SERVER_OFFLINE");
        }
    }
}
