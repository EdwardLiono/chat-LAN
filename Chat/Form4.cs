using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace Test
{
    public partial class Form4 : Form
    {
        TcpClient tcpClient;
        string ID;
        public Form4(TcpClient client, string _ID)
        {
            InitializeComponent();

            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.tcpClient = client;
            this.ID = _ID;
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] message = Encoding.Unicode.GetBytes("find;false;"+findID.Text+";");
            Array.Resize(ref message, 8192);
            tcpClient.GetStream().BeginWrite(message, 0, 8192, null, null);
        }

        class data
        {
            public string name;
            public string id;
            public string division;
            
            public data(string _name, string _id, string _division)
            {
                name = _name;
                id = _id;
                division = _division;
            }
        }
        private delegate void SafeCallDelegate(object obj, Panel panel);
        public void addControllSafe<T>(object add, Panel panel)
        { 
            dynamic obj = (T)add;
            if (panel.InvokeRequired)
            {
                var d = new SafeCallDelegate(addControllSafe<T>);
                panel.Invoke(d, new object[] { add, panel });
            }
            else
            {
                panel.Controls.Add(obj);
            }
        }
        private delegate void SafeCallDelegatePanel(Panel panel);
        public void clearControllSafe(Panel panel)
        {
            if (panel.InvokeRequired)
            {
                var d = new SafeCallDelegatePanel(clearControllSafe);
                panel.Invoke(d, new object[] { panel });
            }
            else
            {
                panel.Controls.Clear();
            }
        }
        public void findResult(string res)
        {
            clearControllSafe(panel1);
            string[] dataSplit = res.Split(';');
            int num = (dataSplit.Length - 2) / 3;
            int locminus = 0;
            for (int i = 0; i < num; i++)
            {
                if(dataSplit[3 * i + 2] == ID)
                {
                    locminus = 1;
                    continue;
                }
                Panel p= new Panel();
                p.Size = new Size(244, 60);
                p.Location= new Point(0,60*(i-locminus));
                p.BackColor = Color.White;
                p.BorderStyle = BorderStyle.FixedSingle;
                p.Visible= true;
                
                Label n= new Label();
                n.Text = dataSplit[3*i+3]+"-"+dataSplit[3*i+4];
                n.AutoSize = true;
                n.MaximumSize = new Size(175, 30);
                n.Location = new Point(10, 10);
                n.Font= new Font("Microsoft Sans Serif", 10);
                n.ForeColor = Color.Black;
                n.Visible= true;
                p.Controls.Add(n);

                Label idlabel= new Label();
                idlabel.Text = dataSplit[3 * i + 2];
                idlabel.AutoSize = true;
                idlabel.MaximumSize = new Size(175, 30);
                idlabel.Location = new Point(10, n.Height + 20);
                idlabel.ForeColor = Color.Black;
                idlabel.Visible= true;
                p.Controls.Add(idlabel);

                Button button = new Button();
                button.Text = "Add";
                button.Size = new Size(50, 30);
                button.Location = new Point(184, 15);
                button.Tag = new data(dataSplit[3 * i + 3], dataSplit[3 * i + 2], dataSplit[3 * i + 4]);
                button.Click += ButtonClick;
                p.Controls.Add(button);
                addControllSafe<Panel>(p, panel1);
            }
        }
        private void ButtonClick(object sender, EventArgs e)
        {
            Button s = (Button)sender;
            data d = (data)s.Tag;
            byte[] message = Encoding.Unicode.GetBytes("add;" + d.id + ";");
            Array.Resize(ref message, 8192);
            tcpClient.GetStream().BeginWrite(message, 0, 8192, null, null);
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        private void findID_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
