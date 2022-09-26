using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class ClientSide
    {
        public ClientSide()
        {

        }
        public void ReceiveClient(Socket clientSocket)
        {
            bool notStillTalking = false;
            try
            {
                string toSend = string.Empty;
                byte[] message = new byte[1024];

                byte[] messageReceived = new byte[1024];

                int byteRecv;
                do
                {
                    toSend = string.Empty;
                    message = new byte[1024];

                    messageReceived = new byte[1024];

                    byteRecv = clientSocket.Receive(messageReceived);

                    (int l, int r) = Console.GetCursorPosition();
                    Console.SetCursorPosition(0, r);
                    Console.WriteLine(
                          Encoding.ASCII.GetString(messageReceived,
                          0, byteRecv));
                    Console.Write("Message? ");
                } while (!notStillTalking);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ExecuteClient(IPAddress ipAddr, IPEndPoint localEndPoint)
        {
            bool notStillTalking = false;
            Socket sender = new Socket(ipAddr.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sender.Connect(localEndPoint);
                Thread t = new Thread(new ThreadStart(() => ReceiveClient(sender)));
                t.Start();

                Console.WriteLine("Socket connected to -> {0}\n",
                          sender.RemoteEndPoint.ToString());
                string toSend = string.Empty;
                byte[] message = new byte[1024];
                do
                {
                    Console.Write("Message? ");
                    toSend = Console.ReadLine();

                    message = Encoding.UTF8.GetBytes(toSend + "<EOF>");
                    sender.Send(message);

                    notStillTalking = toSend.Contains("#NotChatting");
                } while (!notStillTalking);
                //byte[] messageReceived = new byte[1024];

                //int byteRecv = sender.Receive(messageReceived);
                //Console.WriteLine("Message from Server -> {0}",
                //      Encoding.ASCII.GetString(messageReceived,
                //      0, byteRecv));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            } finally
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }
    }
}
