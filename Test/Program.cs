using Net.Ntp;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var udpClient = new UdpClient("ntp1.gatech.edu", 123);
            //var ipEndpoint = new IPEndPoint(IPAddress.Parse("130.207.165.28"), 123);
            //var reqBytes = Encoding.ASCII.GetBytes("Anybody there?");
            //udpClient.Send(reqBytes, reqBytes.Length);
            //udpClient.Connect(ipEndpoint);
            //udpClient.Send(System.Text.Encoding.ASCII.GetBytes(""), 0, ipEndpoint);
            //var received = udpClient.Receive(ref ipEndpoint);
            //var rcvString = Convert.ToString(received);
            var req = new NtpRequest()
            {
                LeapIndicator = LeapIndicator.NoLeapSecondAdjustment,
                Mode = Mode.Client,
                Stratum = Stratum.PrimaryServer,
                VersionNumber = 3,
                OriginateTimestampUtc = DateTime.UtcNow
            };
            var bytes = req.GetBytes();

            //const string ntpServer = "ntp2.gatech.edu";
            const string ntpServer = "time.windows.com";
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            var sw = new Stopwatch();
            sw.Start();
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(bytes);
                socket.Receive(bytes);
                socket.Close();
            }
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;
            var response = NtpResponse.ParseBytes(bytes);
        }
    }
}
