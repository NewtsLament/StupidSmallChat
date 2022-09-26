using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;

namespace ChatServer
{
    public class ServerSide
    {
        List<Message> messages = new List<Message>();
        readonly object messagesLock = new object();
        Dictionary<Socket, bool> isRunning = new Dictionary<Socket, bool>();
        readonly object isRunningtLock = new object();
        bool continueExecution = true;
        readonly object continueExecutionLock = new object();

        public IPHostEntry GetHost()
        {
            return Dns.GetHostEntry(Dns.GetHostName());
        }

        public IPAddress GetIpAddress(IPHostEntry ipHost)
        {
            return ipHost.AddressList[0];
        }

        public IPEndPoint GetLocalEndpoint(IPAddress ipAddr)
        {
            return new IPEndPoint(ipAddr, 11111);
        }

        public void Sender(IPHostEntry ipHost, IPAddress ipAddr, IPEndPoint localEndPoint, Socket clientSocket)
        {
            List<Message> localMessages = new List<Message>();
            bool notStillChatting = false;
            try
            {
                byte[] bytes = new Byte[1024];
                string? data = null;
                byte[] message = new byte[1024];
                int localMessageLength, globalMessageLength;
                do
                {
                    localMessageLength = localMessages.Count;
                    globalMessageLength = messages.Count;
                    if (localMessageLength < globalMessageLength)
                    {
                        for (int i = localMessageLength; i < globalMessageLength; i++)
                        {
                            message = Encoding.UTF8.GetBytes(messages[i].ToString());
                            clientSocket.Send(message);
                            localMessages.Add(messages[i]);
                        }
                    }

                    Thread.Sleep(500);
                    notStillChatting = !isRunning[clientSocket];
                    //byte[] outMessage = Encoding.ASCII.GetBytes("GG");
                    //clientSocket.Send(outMessage);
                } while (!notStillChatting);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Receiver(IPHostEntry ipHost, IPAddress ipAddr, IPEndPoint localEndPoint, Socket clientSocket)
        {
            bool notStillChatting = false;
            lock (isRunningtLock)
            {
                isRunning.Add(clientSocket, true);
            }
            try
            {
                byte[] bytes = new Byte[1024];
                string? data = null;
                do {
                    bytes = new byte[1024];
                    data = null;

                    do
                    {
                        int numByte = clientSocket.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes,
                            0, numByte);
                    } while (data.IndexOf("<EOF>") < -1);

                    Message message = new Message(clientSocket.RemoteEndPoint.ToString(), data.Substring(0,data.Length-5));
                    lock (messagesLock)
                    {
                        messages.Add(message);
                    }

                    Console.WriteLine(message);

                    notStillChatting = message.Content.Contains("#NotChatting");

                    //byte[] outMessage = Encoding.ASCII.GetBytes("GG");
                    //clientSocket.Send(outMessage);
                } while (!notStillChatting);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            } finally
            {
                lock (isRunningtLock)
                {
                    isRunning[clientSocket] = false;
                }
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        public void ServerConsole()
        {
            Console.WriteLine("Type [#quit] + [enter] to exit server \"gracefully\"");

            if (Console.ReadLine().Contains("#quit"))
            {
                lock (continueExecutionLock)
                {
                    continueExecution = false;
                }
            }
        }
        public void Run()
        {
            Thread t1 = new Thread(StartServerAsync);
            t1.Start();
            Thread t2 = new Thread(ServerConsole);
            t2.Start();
            while (continueExecution)
            {
                Thread.Sleep(1000);
            }
        }
        public void StartServerAsync()
        {

            IPHostEntry ipHost = GetHost();
            IPAddress ipAddr = GetIpAddress(ipHost);
            IPEndPoint localEndPoint = GetLocalEndpoint(ipAddr);


            Socket listener = new Socket(ipAddr.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);

            do
            {
                Console.WriteLine("Waiting for a connection");

                Socket clientSocket = listener.Accept();
                Thread t1 = new Thread(() => Receiver(ipHost, ipAddr, localEndPoint, clientSocket));
                t1.Start();
                Thread t2 = new Thread(() => Sender(ipHost, ipAddr, localEndPoint, clientSocket));
                t2.Start();
                Console.WriteLine($"Thread {t1.ManagedThreadId} started.");
            } while (continueExecution);
        }
    }
}
