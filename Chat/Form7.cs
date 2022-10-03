using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;

namespace Test
{
    public partial class Form7 : Form
    {
        TcpClient tcpClient;
        string ID;
        public Form7(TcpClient client, string _ID)
        {
            InitializeComponent();

            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.tcpClient = client;
            this.ID = _ID;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void Form7_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Visible=false;
            e.Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] message = Encoding.Unicode.GetBytes("adminbroadcast;"+ID+";" + BroadcastText.Text + ";");
            Array.Resize(ref message, 8192);
            tcpClient.GetStream().BeginWrite(message, 0, 8192, null, null);

            BroadcastText.Text = "";
        }

        private void Form7_Load(object sender, EventArgs e)
        {

        }
    }
}
