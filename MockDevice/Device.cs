using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Newtonsoft.Json;

namespace MockDevice
{
    public struct TemperatureData
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public float Temperature { get; set; }
        public DateTime Time { get; set; }
    }

    public class Device
    {
        private const string IpRequestString = "IP_REQUEST";
        private const int TcpPort = 8000;

        private readonly string _macAddress;
        private readonly string _name;
        private string _hostIp = string.Empty;
        private Timer _timer;

        public Device(string macAddress, string name)
        {
            _macAddress = macAddress;
            _name = name;
        }

        public void Connect()
        {
            SendIpRequest();

            while (string.IsNullOrEmpty(_hostIp))
            {
                _hostIp = GetHostIp();
                _hostIp = "192.168.0.100"; // HACK
            }

            _timer = new Timer(1000 * 60); // 1 minute
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public string GetHostIp()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 4568);

            socket.Bind(ipEndPoint);

            IPAddress ip = IPAddress.Parse("224.5.6.8");

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

            byte[] b = new byte[1024];

            if (socket.Poll(10000000, SelectMode.SelectRead))
            {
                socket.Receive(b);
            }

            string str = Encoding.UTF8.GetString(b, 0, b.Length).Trim().Replace("\0", string.Empty);

            if (str.Equals(IpRequestString))
            {
                return string.Empty;
            }

            Console.WriteLine($"[Device] Received host ip: {str}");

            return str;
        }

        public void SendIpRequest()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress ip = IPAddress.Parse("224.5.6.7");

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 5);

            IPEndPoint ipep = new IPEndPoint(ip, 4567);
            socket.Connect(ipep);

            var bytes = Encoding.UTF8.GetBytes(IpRequestString);

            Console.WriteLine($"[Device] Sending ip request: {IpRequestString}");

            socket.Send(bytes, bytes.Length, SocketFlags.None);

            socket.Close();
        }

        private float GetTemperature()
        {
            var max = 30f;
            var min = -10f;

            var random = new Random();
            double val = (random.NextDouble() * (max - min) + min);
            return (float)val;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(_hostIp, TcpPort);

            using (var stream = tcpClient.GetStream())
            {
                var data = new TemperatureData { MacAddress = _macAddress, Name = _name, Temperature = GetTemperature(), Time = DateTime.Now };

                var dataString = JsonConvert.SerializeObject(data);

                Console.WriteLine($"[Device] Sending data: {dataString}");

                var responseBytes = Encoding.UTF8.GetBytes(dataString);

                stream.Write(responseBytes, 0, responseBytes.Length);

                tcpClient.Close();
            }
        }
    }
}