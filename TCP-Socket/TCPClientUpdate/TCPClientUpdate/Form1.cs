using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

namespace TCPClientUpdate
{
    public partial class Form1 : Form
    {
        private string sExe;//可执行程序名

        public Form1()
        {
            InitializeComponent();
            sExe = textBox_exe.Text.Trim();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[1024];
            Socket skClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //通过配置文件更改端口号，使用时请根据具体情况更改ip地址
            string sIpServer = ConfigurationManager.AppSettings["IPServer"].ToString();
            int port = Int32.Parse(ConfigurationManager.AppSettings["PortServer"].ToString());
            IPEndPoint IPAndPoint = new IPEndPoint(IPAddress.Parse(sIpServer), port);

            try
            {
                //因为客户端只是用来向特定的服务器发送信息，
                //所以不需要绑定本机的IP和端口。不需要监听。
                skClient.Connect(IPAndPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("无法连接到服务端");
                Console.WriteLine(ex.Message);
                return;
            }

            int iReceiveLength = skClient.Receive(data);
            //字节数组到字符串
            string sData = Encoding.ASCII.GetString(data, 0, iReceiveLength);
            Console.WriteLine(sData);
            //发送数据
            if (!string.IsNullOrEmpty(sExe))
            {
                //客户端发送数据
                skClient.Send(Encoding.ASCII.GetBytes(sExe));
            }

            Console.WriteLine("断开与服务端的连接...");
            skClient.Shutdown(SocketShutdown.Both);
            skClient.Close();
            Console.ReadLine();
        }
    }
}
