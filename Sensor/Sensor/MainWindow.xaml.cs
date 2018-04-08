using Sensor.Model;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sensor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConcurrentList<Client> clientList;

        public MainWindow()
        {
            InitializeComponent();
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) => UpdateApp();
            timer.Start();
            new Task(() =>
            {
                Server();
            }).Start();
        }

        private async Task Server()
        {
            IPAddress ipAd = IPAddress.Parse("127.0.0.1");
            // use local m/c IP address, and 
            // use the same in the client

            /* Initializes the Listener */
            TcpListener myList = new TcpListener(ipAd, 8080);

            /* Start Listeneting at the specified port */
            myList.Start();
            Dictionary<int, Tuple<string, UIElement>> dict = new Dictionary<int, Tuple<string, UIElement>>();
            clientList = new ConcurrentList<Client>();

            Object locker = new object();

            while (true)
            {
                Socket s = myList.AcceptSocket();

                var childSocketThread = new Task(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine("The server is running at port 8001...");
                            Console.WriteLine("The local End point is  :" +
                                              myList.LocalEndpoint);
                            Console.WriteLine("Waiting for a connection.....");


                            Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                            byte[] b = new byte[100];
                            int k = s.Receive(b);
                            // b = ReadBySize(s);
                            Console.WriteLine("Recieved...");
                            StringBuilder str = new StringBuilder();
                            for (int i = 0; i < b.Count(); i++)
                                str.Append(Convert.ToChar(b[i]));
                            Console.WriteLine(str);
                            var trimPackets = GetAllPackets(GetListPackets(str));

                            foreach (var packet in trimPackets)
                            {
                                if (clientList.Any(x=>x.Name == packet.Item1))
                                {
                                    var temp = clientList.FirstOrDefault(x=>x.Name == packet.Item1);
                                    Dispatcher.Invoke(() =>
                                    {
                                        temp.UIText.Text = packet.ToString();
                                    });

                                    clientList[clientList.IndexOf(clientList.FirstOrDefault(x => x.Name == packet.Item1))] = new Client { Name = packet.Item1, Value = packet.Item2, UIText = temp.UIText };
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        TextBlock tb = new TextBlock();
                                        tb.Width = 100;
                                        //tb.Margin = new Thickness(20);
                                        tb.Text = packet.ToString();

                                        // stck.Children.Add(tb);
                                        clientList.Add(new Client { Name = packet.Item1, Value = packet.Item2, UIText = tb});
                                    });
                                }
                            }


                            //Dispatcher.Invoke(async() =>
                            //    {
                            //        //var children = stck.Children;
                            //        //foreach (var element in children)
                            //        //{
                            //        //    var parseTuple = ParseStringToTuple((element as TextBlock).Text);

                            //        //}
                            //        //if (stck.Children.Contains(keyValue.Value.Item2))
                            //        //{
                            //        //    var children = stck.Children;
                            //        //    foreach (var element in children)
                            //        //    {
                            //        //        if (element == keyValue.Value.Item2)
                            //        //        {
                            //        //            element = ParseStringToTuple((keyValue.Value.Item2 as TextBlock).Text);
                            //        //        }
                            //        //    }
                            //        //}
                            //       // lock (locker)
                            //        {
                            //           await UpdateApp(clientList);
                            //            //var tempCollection = clientList.ToList();
                                        
                            //        }
                            //    });


                            ASCIIEncoding asen = new ASCIIEncoding();
                            s.Send(asen.GetBytes("The string was recieved by the server."));
                            Console.WriteLine("\nSent Acknowledgement");
                        }
                        catch (Exception e) when (e is  FileLoadException)
                        {
                            //s.Close();
                            Console.WriteLine("Disconnect");
                            //break;
                        }
                        //catch
                        //{
                        //    break;
                        //}
                        /* clean up */
                        //s.Close();
                        // myList.Stop();
                    }
                });
                childSocketThread.Start();
            }

        }

        private void UpdateApp()
        {
            Dispatcher.Invoke(()=> {
                stck.Children.Clear();
                for (var i = 0; i < clientList.Count; i++)
                {
                    stck.Children.Add(clientList[i].UIText);
                    stck.UpdateLayout();
                }
            });
        }

        private byte[] ReadBySize(Socket socket, int size = 100)
        {
            var readEvent = new AutoResetEvent(false);
            var buffer = new byte[size]; //Receive buffer
            var totalRecieved = 0;
            do
            {
                var recieveArgs = new SocketAsyncEventArgs()
                {
                    UserToken = readEvent
                };
                recieveArgs.SetBuffer(buffer, totalRecieved, size - totalRecieved);//Receive bytes from x to total - x, x is the number of bytes already recieved
                recieveArgs.Completed += recieveArgs_Completed;
                socket.ReceiveAsync(recieveArgs);
                readEvent.WaitOne();//Wait for recieve

                totalRecieved += recieveArgs.BytesTransferred;

            } while (totalRecieved != size);//Check if all bytes has been received
            return buffer;
        }

        void recieveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            var are = (AutoResetEvent)e.UserToken;
            are.Set();
        }

        private List<Tuple<int, int>> GetAllPackets(List<string> list)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            foreach (var packet in list)
            {
                try
                {
                    var trimPacket = packet.Split(',');
                    result.Add(new Tuple<int, int>(int.Parse(trimPacket[0]), int.Parse(trimPacket[1])));
                }
                catch
                {
                    continue;
                }
            }
            return result;
        }

        private List<string> GetListPackets(StringBuilder str)
        {
            var packet = str.Replace("{", "").Replace("}", "").ToString();
            return packet.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();

        }

        public Tuple<int, int> ParseStringToTuple(string inputString)
        {
            var trimElement = inputString.Replace("(", "").Replace(")", "").Split(',').ToList();
            return new Tuple<int, int>(int.Parse(trimElement[0]), int.Parse(trimElement[1]));
        }
    }




   
}
