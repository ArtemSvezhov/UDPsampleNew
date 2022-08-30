using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace UDPclient
{
    class Program
    {
        private static IPAddress remoteIPAddress;
        private static int remotePort;
        private static int localPort;

        public class JsonLIST
        {
            public string PlaceN { get; set; }
            public string OperName { get; set; }
            public string Percent { get; set; }
        }

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Входные данные Локальный для первго 8001/8002 для воторго ИП 127.0.0.1\n");
                // Получаем данные, необходимые для соединения
                //Console.WriteLine("Укажите локальный порт");
                localPort = Convert.ToInt16(8002);

                //Console.WriteLine("Укажите удаленный порт");
                remotePort = Convert.ToInt16(8001);

                Console.WriteLine("Укажите удаленный IP-адрес");
                remoteIPAddress = IPAddress.Parse("127.0.0.1");//Any;

                // Создаем поток для прослушивания
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                while (true)
                {
                    Send(Console.ReadLine());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        //private static System.Timers.Timer aTimer;

        private static void Send(string datagram)
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();

            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            try
            {
                // if чисто для проверки и перегрузок потом просто убрать

                if (datagram.ToLower() == "test")
                {
                    Random rnd = new Random();

                    while (true)
                    {
                        byte[] bytes;
                        var JsonLISTprim = new JsonLIST
                        {
                            PlaceN = Convert.ToString(rnd.Next(1, 11)),
                            OperName = Convert.ToString("Initial"),
                            Percent = Convert.ToString(rnd.Next(100))
                        };
                        datagram = JsonSerializer.Serialize(JsonLISTprim); ;
                        bytes = Encoding.BigEndianUnicode.GetBytes(datagram);
                        Thread.Sleep(50);
                        // Отправляем данные
                        sender.Send(bytes, bytes.Length, endPoint);

                    }
                }
                else
                {
                    // Преобразуем данные в массив байтов
                    byte[] bytes = Encoding.BigEndianUnicode.GetBytes(datagram);

                    // Отправляем данные в виде массива
                    sender.Send(bytes, bytes.Length, endPoint);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }

        // Потом добить приём CANCEL, Пока не трогаю клиентскую часть
        public static void Receiver()
        {
            // Создаем UdpClient для чтения входящих данных
            UdpClient receivingUdpClient = new UdpClient(localPort);

            IPEndPoint RemoteIpEndPoint = null;
            while (true)
            {
                // Ожидание дейтаграммы
                byte[] receiveBytes = receivingUdpClient.Receive(
                   ref RemoteIpEndPoint);

                // Преобразуем и отображаем данные
                string returnData = Encoding.BigEndianUnicode.GetString(receiveBytes);

                //{"PlaceN":"1","OperName":"Initialization","Percent":"1"}

                //если нужно будет посмотреть вход

                //Console.WriteLine(returnData);
                try
                {
                    if (returnData == "stop")
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                }
            }

        }
    }
}

