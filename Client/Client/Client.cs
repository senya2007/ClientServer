using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        TcpClient tcpClient;
        NetworkStream stream;
        IPAddress address;
        int port;

        public Client(string address, int port)
        {
            ParseIPAddress(address);
            this.port = port;
        }

        private void CheckForException(Action methodForCheck)
        {
            try
            {
                methodForCheck.Invoke();
            }
            catch (Exception e) when (e.InnerException is SocketException)
            {
                Console.WriteLine("Connect failed");
                CheckForException(() => 
                        OpenPort()
                    );
            }
            catch (Exception e) when (e is FormatException)
            {
                address = IPAddress.Parse("127.0.0.1");
                Console.WriteLine("Cannot parse IP");
            }
        }
        private void ParseIPAddress(string address)
        {
             CheckForException(() => 
                this.address = IPAddress.Parse(address)
             );
        }

        public void Run(int numberClient, int count)
        {
            CheckForException(() => 
                    OpenPort()
                );
            for (int cycle = 0; cycle < count; cycle++)
            {
                Random rnd = new Random();
                var sendString = $"{numberClient},{rnd.Next(0, 100) + numberClient.ToString()};";
                CheckForException(() => {
                    SendData(sendString);
                    Console.WriteLine(ResponseFromServer());
                        });

                Console.WriteLine(sendString);                
                Thread.Sleep(800);
            }
            ClosePort();
        }

        private bool OpenPort()
        {
            tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(address, port);
            connectTask.Wait(5000);

            stream = tcpClient.GetStream();

            return stream.CanWrite;
        }

        private void SendData(String str)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] responseByteArray = encoder.GetBytes(str);
            Console.WriteLine("Transmitting.....");

            stream.Write(responseByteArray, 0, responseByteArray.Length);
        }

        private string ResponseFromServer()
        {
            byte[] bytes = new byte[100];
            int count = stream.Read(bytes, 0, 100);

            StringBuilder resultString = new StringBuilder();
            for (int i = 0; i < count; i++)
                resultString.Append(Convert.ToChar(bytes[i]));

            return resultString.ToString();
        }

        private void ClosePort()
        {
            stream.Close();
            tcpClient.Close();
        }
    }
}
