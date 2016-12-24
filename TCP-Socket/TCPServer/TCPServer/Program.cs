using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int iReceiveLength;//客户端发送的信息长度
            byte[] data = new byte[1024];//缓存客户端发送的信息，Socket传递的信息必须为字节数组。
            
            int port = Int32.Parse(ConfigurationManager.AppSettings["PortServer"].ToString());
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket skServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            skServer.Bind(ipEndPoint);
            skServer.Listen(10);
            Console.WriteLine("等待客户连接...");

            Socket skClient = skServer.Accept();
            IPEndPoint ipClient = (IPEndPoint)skClient.RemoteEndPoint;
            Console.WriteLine(string.Format("与客户端相连接: {0} 端口号: {1}",ipEndPoint.Address,ipEndPoint.Port));

            string sWelcome = "Welcome Here!";
            data = Encoding.ASCII.GetBytes(sWelcome);
            //发送字节信息到客户端
            skClient.Send(data, data.Length, SocketFlags.None);
            string sData;
            //不断从客户端获取信息
            while (true)
            {
                data = new byte[1024];
                //服务端接收数据并放入字节数组
                iReceiveLength = skClient.Receive(data);
                //字节数组到字符串
                sData = Encoding.ASCII.GetString(data, 0, iReceiveLength);
                Console.WriteLine("接收字符长度:{0}", iReceiveLength);
                if(iReceiveLength == 0)
                {
                    break;
                }
                Console.WriteLine("接收字符:{0}", sData);
                //服务端发送数据
                skClient.Send(data,iReceiveLength,SocketFlags.None);
            }
            Console.WriteLine("断开与" + ipClient.Address.ToString() + "的连接");
            skClient.Close();
            skServer.Close();
        }
    }
}
