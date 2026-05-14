using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MISS.Browser.Services
{
    public class NetworkingService
    {
        private const string DNS_IP = "127.0.0.1";
        private const int DNS_PORT = 5053;

        public async Task<(string? ip, int port)> ResolveDNSAsync(string domain)
        {
            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.ReceiveTimeout = 2000; // 2 second timeout
                    udpClient.Connect(DNS_IP, DNS_PORT);
                    byte[] sendBytes = Encoding.ASCII.GetBytes(domain);
                    await udpClient.SendAsync(sendBytes, sendBytes.Length);

                    var receiveTask = udpClient.ReceiveAsync();
                    if (await Task.WhenAny(receiveTask, Task.Delay(2000)) == receiveTask)
                    {
                        var result = await receiveTask;
                        string response = Encoding.ASCII.GetString(result.Buffer);

                        if (response == "NOT_FOUND")
                            return ("NOT_FOUND", 0);

                        string[] parts = response.Split(':');
                        return (parts[0], int.Parse(parts[1]));
                    }
                    else
                    {
                        throw new Exception("DNS_TIMEOUT");
                    }
                }
            }
            catch (SocketException)
            {
                throw new Exception("DNS_OFFLINE");
            }
        }

        public async Task<string> FetchWebPageAsync(string ip, int port)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    var connectTask = tcpClient.ConnectAsync(ip, port);
                    if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
                    {
                        throw new Exception("SERVER_TIMEOUT");
                    }
                    await connectTask;

                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        string request = "GET / HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n";
                        byte[] sendBytes = Encoding.ASCII.GetBytes(request);
                        await stream.WriteAsync(sendBytes, 0, sendBytes.Length);

                        byte[] buffer = new byte[8192];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string fullResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                        int headerEnd = fullResponse.IndexOf("\r\n\r\n");
                        if (headerEnd != -1)
                            return fullResponse.Substring(headerEnd + 4);
                        
                        return fullResponse;
                    }
                }
            }
            catch (SocketException)
            {
                throw new Exception("SERVER_OFFLINE");
            }
        }
    }
}
