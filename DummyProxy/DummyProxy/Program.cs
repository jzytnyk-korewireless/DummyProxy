using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace dummyProxy
{
    internal class Program
    {

        private const int OmegaPort = 55555;
        private const int KorePort = 33333;


        private static void Main()
        {
            OmegaRadius();
        }

        private static void OmegaRadius()
        {
            // Create a UDP socket to listen for incoming authentication requests.
            Console.WriteLine("OMEGA: Initializing authentication socket (IP {0}, port {1})...", IPAddress.Loopback, OmegaPort);
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //this socket is bound to port 55555 and will only find packets sent to that port
                socket.Bind(new IPEndPoint(IPAddress.Loopback, OmegaPort));

                var ggsn = new Thread(GgsnServer);
                ggsn.Start();

                var kore = new Thread(KoreProxy);
                kore.Start();

                while (true)
                {
                    var ggsnBuffer = new UdpPacketBuffer();
                    var koreBuffer = new UdpPacketBuffer();
                    Console.WriteLine("OMEGA: waiting for packet from ggsn");

                    socket.ReceiveFrom(ggsnBuffer.Data, ref ggsnBuffer.RemoteEndPoint);

                    var ggsnEndPoint = ((IPEndPoint) ggsnBuffer.RemoteEndPoint);
                    var ip = ggsnEndPoint.Address;
                    var port = ggsnEndPoint.Port;
                    var msg = Encoding.Default.GetString(ggsnBuffer.Data);

                    Console.WriteLine("OMEGA: Received ggsn msg: {0}:{1} data: {2}", ip, port,msg);

                    if (msg.ToLower().StartsWith("kore"))
                    {
                        Console.WriteLine("OMEGA: sending proxy msg to KORE");
                        koreBuffer.Data = ggsnBuffer.Data;
                        
                        socket.SendTo(koreBuffer.Data, new IPEndPoint(IPAddress.Loopback, KorePort));
                        socket.ReceiveFrom(koreBuffer.Data, ref koreBuffer.RemoteEndPoint);
                        var koreEndPoint = ((IPEndPoint)koreBuffer.RemoteEndPoint);
                        var koreip = koreEndPoint.Address;
                        var koreport = koreEndPoint.Port;
                        var koremsg = Encoding.Default.GetString(koreBuffer.Data);
                        Console.WriteLine("OMEGA: Received proxy response: {0}:{1} data: {2}", koreip, koreport, koremsg);

                        ggsnBuffer.Data = Encoding.Default.GetBytes(koremsg);

                    }
                    else
                    {
                        ggsnBuffer.Data = Encoding.Default.GetBytes("Accepted");
                    }

                    //we are using socket defined above, this causes it to go out from port 55555
                    //(i think) this is how omegaRadius will work
                    //with 1 port defined for accounting and 1 for authentication
                    //if we send to KORE with the same socket, it should go out on the same port it is listening on
                    //if kore returns the message to the same port then it would work.

                    
                    socket.SendTo(ggsnBuffer.Data, ggsnEndPoint);

                    Console.WriteLine("OMEGA: sent the bytes back to ggsn");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: {0}", ex.Message);
            }
            Console.ReadLine();
        }

        private static void GgsnServer()
        {
            try
            {
                while (true)
                {

                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var buffer = new UdpPacketBuffer();

                    Thread.Sleep(5); //sleep a tad to get msg on bottom
                    Console.WriteLine("GGSN: Enter your packet:");
                    var msg = Console.ReadLine();
                    var bytes = Encoding.Default.GetBytes(msg);

                    Console.WriteLine("GGSN: sending msg to OMEGA..");                    
                    //we do not bind this socket to a port, so it picks one at random
                    socket.SendTo(bytes, new IPEndPoint(IPAddress.Loopback, OmegaPort));

                    Console.WriteLine("GGSN: waiting for response from OMEGA");

                    //will listen on that (random) port for a response
                    socket.ReceiveFrom(buffer.Data, ref buffer.RemoteEndPoint);

                    var remoteEndPoint = ((IPEndPoint) buffer.RemoteEndPoint);
                    var ip = remoteEndPoint.Address;
                    var port = remoteEndPoint.Port;

                    Console.WriteLine("GGSN: Received from: {0}:{1} data: {2}", ip, port,
                        Encoding.Default.GetString(buffer.Data));

                    socket.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("GGSN: An error occurred: {0}", ex.Message);
            }
            Console.ReadLine();
        }




        private static void KoreProxy()
        {            

            try
            {

                while (true)
                {
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var buffer = new UdpPacketBuffer();

                    socket.Bind(new IPEndPoint(IPAddress.Loopback, KorePort));
                    Console.WriteLine("KORE: waiting for packet");

                    socket.ReceiveFrom(buffer.Data, ref buffer.RemoteEndPoint);

                    var remoteEndPoint = ((IPEndPoint) buffer.RemoteEndPoint);
                    var ip = remoteEndPoint.Address;
                    var port = remoteEndPoint.Port;

                    Console.WriteLine("KORE: Received from: {0}:{1} data: {2}", ip, port,
                        Encoding.Default.GetString(buffer.Data));

                    buffer.Data = Encoding.Default.GetBytes("Accepted-KORE");

                    socket.SendTo(buffer.Data, buffer.RemoteEndPoint);

                    Console.WriteLine("KORE: sent the bytes back to OMEGA");

                    socket.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("KORE: An error occurred: {0}", ex.Message);
            }
            Console.ReadLine();
        }

    }


public class UdpPacketBuffer
    {
        public const int BufferSize = 128;
        public byte[] Data;
        public int DataLength;
        public EndPoint RemoteEndPoint;
        
        public UdpPacketBuffer()
        {
            Data = new byte[BufferSize];
            RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }
    }
}
