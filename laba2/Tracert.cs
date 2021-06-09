using System;
using System.Net;
using System.Net.Sockets;

namespace CNaS_Lab_2
{
    public class Tracert
    {
        private const Int32 Max_Hop = 30;
        private const Int32 Max_Package = 3;
        private const Int32 Max_TimeOut = 3000;

        public void Start(string host, bool showDNS = false)
        {
            IPHostEntry ipHost;
            try
            {
                ipHost = Dns.GetHostEntry(host);
            }
            catch (SocketException)
            {
                Console.WriteLine("Не удается разрешить системное имя узла " + host + ".");
                return;
            }

            // Адрес + порт
            IPEndPoint ipPoint = new IPEndPoint(ipHost.AddressList[0], 0);
            // Адрес
            EndPoint endPoint = ipPoint;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            // Операции отправки будут отключены, если подтверждение не придет в течение .. миллисекунд
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Max_TimeOut);

            Console.WriteLine("Трассировка маршрута к " + host + $" [ {ipPoint.Address} ]");
            Console.WriteLine("с максимальным количеством прыжков " + Max_Hop + ":\n");

            byte[] data = Icmp.CreateIcmpPackage();
            byte[] receivedData;
            Int32 TTL = 1;
            bool isEndPointReached = false;

            for (Int32 i = 0; i < Max_Hop; i++)
            {
                Int32 errorAnswer = 0; 
                Console.Write("{0,2}", i + 1);

                // Каждый раз устанавливается новое значение TTL
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, TTL++);

                receivedData = new byte[512];

                for (Int32 j = 0; j < Max_Package; j++)
                {
                    try
                    {
                        DateTime timeStart = DateTime.Now;
                        socket.SendTo(data, data.Length, SocketFlags.None, ipPoint);
                        socket.ReceiveFrom(receivedData, ref endPoint);
                        TimeSpan deltaTime = (DateTime.Now - timeStart);
                        isEndPointReached = IsDestinationReached(receivedData, (Int32)deltaTime.TotalMilliseconds);
                    }
                    catch (SocketException)
                    {
                        errorAnswer++;
                        Console.Write("{0,10}", "*");                        
                    }
                }

                if (errorAnswer == Max_Package)
                {
                    Console.Write("  Превышен интервал ожидания для запроса.\n");
                }
                else if (showDNS)
                {
                    ShowDns(endPoint);
                }
                else
                {
                    Console.Write($"  { GetIpAddr(endPoint) } \n");
                }

                if (isEndPointReached)
                {
                    Console.WriteLine("\nТрассировка завершена.");
                    break;
                }
            }
        }

        private bool IsDestinationReached(byte[] receivedMessage, Int32 responseTime)
        {
            Int32 receivedType = Icmp.GetIcmpType(receivedMessage);
            if (receivedType == 0)
            {
                Console.Write("{0, 10}", responseTime + " мс");
                return true;
            }
            if (receivedType == 11)
            {
                Console.Write("{0, 10}", responseTime + " мс");
            }
            return false;
        }

        private string GetIpAddr(EndPoint ipAndPort)
        {
            return ipAndPort.ToString().Substring(0, ipAndPort.ToString().Length - 2);
        }

        private void ShowDns(EndPoint endPoint)
        {
            try
            {
                Console.Write($"  { ReverseDns(endPoint) } [ { GetIpAddr(endPoint) } ]\n");
            }
            catch (SocketException)
            {
                Console.Write($"  { GetIpAddr(endPoint) }\n");
            }
        }

        private string ReverseDns(EndPoint endPoint)
        {
            string[] ipAndPort = endPoint.ToString().Split(':');
            return Dns.GetHostEntry(IPAddress.Parse(ipAndPort[0])).HostName;
        }        
    }
}

