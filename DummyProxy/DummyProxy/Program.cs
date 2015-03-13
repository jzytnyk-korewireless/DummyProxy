using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


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
            StartRadiusSocket();
        }



        private static void StartRadiusSocket()
        {
            // Create a UDP socket to listen for incoming authentication requests.
            Console.WriteLine("Initializing authentication socket (IP {0}, port {1})...", RadiusServerIp, RadiusPort);
            

            try
            {
                if (_radiusSocket != null)
                {
                    _radiusSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred closing the radius socket: {0}", ex.Message);                
            }

            try
            {
                _radiusSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    ReceiveBufferSize = SIXTEEN_MEGABYTES
                };

                _radiusSocket.Bind(new IPEndPoint(RadiusServerIp, RadiusPort));

                
                //no idea what this does
                //uint IOC_IN = 0x80000000;
                //uint IOC_VENDOR = 0x18000000;
                //uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                //_radiusSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occurred binding the radius socket: {0}", ex.Message);
                
                throw;
            }


            try
            {
                UDPPacketBuffer buffer = new UDPPacketBuffer();
                
                for (int i = 0; i < 100; i++)
                {
                    Console.WriteLine("waiting for packet #{0}",i);

                    _radiusSocket.Receive(buffer.Data, UDPPacketBuffer.BUFFER_SIZE, SocketFlags.None);

                    //var ip = ((IPEndPoint)buffer.RemoteEndPoint).Address;
                    //var port = ((IPEndPoint)buffer.RemoteEndPoint).Port;

  //                  var ip = ((IPEndPoint) _radiusSocket.RemoteEndPoint);
//                    var port = ((IPEndPoint)_radiusSocket.RemoteEndPoint);


                    Console.WriteLine("Received from:  data: {0}",  Encoding.Default.GetString(buffer.Data));
                    //Console.WriteLine("Received from: {0}:{1} data: {2}",ip,port,Encoding.Default.GetString(buffer.Data));

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred at BeginReceiveFrom: {0}", ex.Message);
            }
            
        }
    }


    
    public class UDPPacketBuffer
    {
        public const int BUFFER_SIZE = 48;
        public byte[] Data;
        public int DataLength;
        public EndPoint RemoteEndPoint;
        
        public UDPPacketBuffer()
        {
            this.Data = new byte[BUFFER_SIZE];
            RemoteEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
        }
    }
}
