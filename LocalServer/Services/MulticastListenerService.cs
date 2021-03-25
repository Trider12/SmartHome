using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace LocalServer.Services
{
    public class MulticastListenerService : BackgroundService
    {
        private const string IpRequestString = "IP_REQUEST";

        private string _localIpAddress;
        private int _networkIndex;

        public MulticastListenerService() : base()
        {
            ConfigureNetwork();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var request = GetIpRequest();

                    if (string.IsNullOrEmpty(request))
                    {
                        continue;
                    }

                    SendLocalIp();
                }
            });
        }

        private void ConfigureNetwork()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in nics)
            {
                //IPInterfaceProperties ip_properties = adapter.GetIPProperties();
                //if (!adapter.GetIPProperties().MulticastAddresses.Any())
                //    continue; // most of VPN adapters will be skipped
                //if (!adapter.SupportsMulticast)
                //    continue; // multicast is meaningless for this type of connection
                //if (OperationalStatus.Up != adapter.OperationalStatus)
                //    continue; // this adapter is off or not connected

                var prop = adapter.GetIPProperties();

                IPv4InterfaceProperties p = prop.GetIPv4Properties();
                if (null == p)
                    continue; // IPv4 is not configured on this adapter

                // HACK
                if (adapter.Name != "Беспроводная сеть")
                {
                    continue;
                }

                foreach (var ip in prop.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _localIpAddress = ip.Address.ToString();
                        break;
                    }
                }

                Console.WriteLine($"[Server] Network: {adapter.Name}");
                Console.WriteLine($"[Server] Ip Address: {_localIpAddress}");

                // now we have adapter index as p.Index, let put it to socket option
                _networkIndex = IPAddress.HostToNetworkOrder(p.Index);
                return;
            }
        }

        private string GetIpRequest()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 4569);

            socket.Bind(ipEndPoint);

            IPAddress ip = IPAddress.Parse("224.5.6.7");

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));
            //socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, _networkIndex);

            byte[] b = new byte[1024];
            socket.Receive(b);
            string str = Encoding.UTF8.GetString(b, 0, b.Length).Trim().Replace("\0", string.Empty);

            if (str.Equals(IpRequestString))
            {
                Console.WriteLine($"[Server] Received ip request: {str}");

                return str;
            }

            return string.Empty;
        }

        //private string GetLocalIp()
        //{
        //    return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(o => o.AddressFamily == AddressFamily.InterNetwork).First().ToString();

        //    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        //    {
        //        socket.Connect("8.8.8.8", 65530);
        //        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        //        var localIP = endPoint.Address.ToString();

        //        return localIP;
        //    }
        //}

        private void SendLocalIp()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress ip = IPAddress.Parse("224.5.6.8");

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, _networkIndex));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 5);

            IPEndPoint ipep = new IPEndPoint(ip, 4570);
            socket.Connect(ipep);

            var bytes = Encoding.UTF8.GetBytes(_localIpAddress);

            Console.WriteLine($"[Server] Sending local ip: {_localIpAddress}");

            socket.Send(bytes, bytes.Length, SocketFlags.None);

            socket.Close();
        }
    }
}