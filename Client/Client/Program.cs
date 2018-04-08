using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        const string IPAddress = "127.0.0.1";
        const int Port = 8080;

        static void Main(string[] args)
        {
            CreateFewClients(100);
            Console.ReadKey();
        }

        private static void CreateFewClients(int countClient)
        {
            Task[] arrayTask = new Task[countClient];
            for (int i = 0; i < countClient; i++)
            {
                var tempI = i;
                new Task(() =>
                  {
                      Client client = new Client(IPAddress, Port);
                      client.Run(tempI, 100);

                  }).Start();
            }
        }
    }
}
