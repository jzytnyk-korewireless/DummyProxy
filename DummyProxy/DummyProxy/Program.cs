using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace dummyProxy
{
    class Program
    {
        static void Main()
        {
            const int listenPort = 55555; 
            var buffer = new UdpPacketBuffer();

            // Create a UDP socket to listen for incoming authentication requests.
            Console.WriteLine("main: Initializing authentication socket (IP {0}, port {1})...", IPAddress.Loopback, listenPort);
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //this socket is bound to port 55555 and (i think) will only find packets sent to that port
                socket.Bind(new IPEndPoint(IPAddress.Loopback, listenPort));
         
                var thr = new Thread(Main2);
                thr.Start();

                Console.WriteLine("main: waiting for packet");

                socket.ReceiveFrom(buffer.Data, ref buffer.RemoteEndPoint);

                var remoteEndPoint = ((IPEndPoint) buffer.RemoteEndPoint);
                var ip = remoteEndPoint.Address;
                var port = remoteEndPoint.Port;
                    
                Console.WriteLine("main: Received from: {0}:{1} data: {2}",ip,port,Encoding.Default.GetString(buffer.Data));

                //we are using socket defined above, this causes it to go out from port 55555
                //(i think) this is how omegaRadius will work
                //with 1 port defined for accounting and 1 for authentication
                //if we send to KORE with the same socket, it (should) go out on the same port it is listening on
                //(if) kore returns the message to the same port then it would work.
                
                socket.SendTo(buffer.Data, remoteEndPoint);

                Console.WriteLine("main: sent the bytes back");

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred at BeginReceiveFrom: {0}", ex.Message);
            }
            Console.ReadLine();
        }

        static void Main2()
        {
            var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var buffer = new UdpPacketBuffer();
            Console.WriteLine("thread: sending msg..");
            //we do not bind this socket to a port, so it picks one at random
            socket2.SendTo(Encoding.Default.GetBytes("weird message"),
                new IPEndPoint(IPAddress.Loopback, 55555));

            try
            {

                Console.WriteLine("thread: waiting for packet");

                //will listen on that (random) port for a response
                socket2.ReceiveFrom(buffer.Data, ref buffer.RemoteEndPoint);

                var remoteEndPoint = ((IPEndPoint)buffer.RemoteEndPoint);
                var ip = remoteEndPoint.Address;
                var port = remoteEndPoint.Port;

                Console.WriteLine("thread: Received from: {0}:{1} data: {2}", ip, port, Encoding.Default.GetString(buffer.Data));

                socket2.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("thread: An error occurred at BeginReceiveFrom: {0}", ex.Message);
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
