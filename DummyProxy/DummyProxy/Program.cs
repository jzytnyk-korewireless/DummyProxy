using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace dummyProxy
{
    class Program
    {
        private static readonly IPAddress RadiusServerIp = IPAddress.Any;
        private const int RadiusPort = 1812;
        private static Socket _radiusSocket;
        private const int SIXTEEN_MEGABYTES = 16777216;

        static void Main(string[] args)
        {
            
            var buffer = new UdpPacketBuffer();
            var counter = 0;

            // Create a UDP socket to listen for incoming authentication requests.
            Console.WriteLine("Initializing authentication socket (IP {0}, port {1})...", RadiusServerIp, RadiusPort);
            try
            {
                _radiusSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    ReceiveBufferSize = SIXTEEN_MEGABYTES
                };

                _radiusSocket.Bind(new IPEndPoint(RadiusServerIp, RadiusPort));
         
                while(true)
                {
                    Console.WriteLine("waiting for packet #{0}",counter);

                    _radiusSocket.ReceiveFrom(buffer.Data, UdpPacketBuffer.BUFFER_SIZE, SocketFlags.None, ref buffer.RemoteEndPoint);

                    var remoteEndPoint = ((IPEndPoint) buffer.RemoteEndPoint);
                    var ip = remoteEndPoint.Address;
                    var port = remoteEndPoint.Port;
                    
                    Console.WriteLine("Received from: {0}:{1} data: {2}",ip,port,Encoding.Default.GetString(buffer.Data));
                    counter++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred at BeginReceiveFrom: {0}", ex.Message);
            }
            
        }
    }


    
    public class UdpPacketBuffer
    {
        public const int BUFFER_SIZE = 128;
        public byte[] Data;
        public int DataLength;
        public EndPoint RemoteEndPoint;
        
        public UdpPacketBuffer()
        {
            this.Data = new byte[BUFFER_SIZE];
            RemoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
        }
    }
}
