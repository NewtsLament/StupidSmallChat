using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatClient;

Console.WriteLine(" __________ ");
Console.WriteLine("|  Client  |");
Console.WriteLine(" ¯¯¯¯¯¯¯¯¯¯ ");

IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
IPAddress ipAddr = ipHost.AddressList[0];
IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);


ClientSide ec = new();


ec.ExecuteClient(ipAddr, localEndPoint);
