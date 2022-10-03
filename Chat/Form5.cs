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
    public partial class Form5 : Form
    {
        TcpClient tcpClient;
        string ID;
        Dictionary<string, data> chosen = new Dictionary<string, data>();

        public Form5(TcpClient client, string _ID)
        {
            InitializeComponent();

            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.tcpClient = client;
            this.ID = _ID;
        }

        private void Form5_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] message = Encoding.Unicode.GetBytes("find;true;" + findID.Text + ";");
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
            int count = 0;
            foreach(data i in chosen.Values)
            {
                Panel p = new Panel();
                p.Size = new Size(244, 60);
                p.Location = new Point(0, 60 * count);
                count++;
                p.BackColor = Color.White;
                p.BorderStyle = BorderStyle.FixedSingle;

                Label n = new Label();
                n.Text = i.name + "-" + i.division;
                n.MaximumSize = new Size(175, 30);
                n.Location = new Point(10, 10);
                p.Controls.Add(n);

                Label idlabel = new Label();
                idlabel.Text = i.id;
                idlabel.MaximumSize = new Size(175, 30);
                idlabel.Location = new Point(10, n.Height + 20);
                p.Controls.Add(idlabel);

                CheckBox checkBox = new CheckBox();
                checkBox.Location = new Point(184, (60 - checkBox.Height) / 2);
                checkBox.Tag = i;
                checkBox.Checked = true;
                checkBox.CheckStateChanged += checkBoxChanged;
                p.Controls.Add(checkBox);
                addControllSafe<Panel>(p, panel1);
            }
            for (int i = 0; i < num; i++)
            {
                if(dataSplit[3 * i + 2] == ID)
                {
                    continue;
                }
                Panel p = new Panel();
                p.Size = new Size(244, 60);
                p.Location = new Point(0, 60 * count);
                count++;
                p.BackColor = Color.White;
                p.BorderStyle = BorderStyle.FixedSingle;

                Label n = new Label();
                n.Text = dataSplit[3 * i + 3] + "-" + dataSplit[3 * i + 4];
                n.MaximumSize = new Size(175, 30);
                n.Location = new Point(10, 10);
                p.Controls.Add(n);

                Label idlabel = new Label();
                idlabel.Text = dataSplit[3 * i + 2];
                idlabel.MaximumSize = new Size(175, 30);
                idlabel.Location = new Point(10, n.Height + 20);
                p.Controls.Add(idlabel);

                CheckBox checkBox = new CheckBox();
                checkBox.Location = new Point(184, (60-checkBox.Height)/2);
                checkBox.Tag = new data(dataSplit[3 * i + 3], dataSplit[3 * i + 2], dataSplit[3 * i + 4]);
                checkBox.CheckStateChanged+=checkBoxChanged;
                p.Controls.Add(checkBox);
                addControllSafe<Panel>(p, panel1);
            }
        }
        private void checkBoxChanged(object sender, EventArgs e)
        {
            CheckBox s = (CheckBox)sender;
            data d = (data)s.Tag;
            if (s.Checked)
            {
                chosen.Add(d.id, d);
            }
            else
            {
                chosen.Remove(d.id);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string msg = "makeGroup;" + GroupName.Text + ";" + ID + ";";
            foreach(data i in chosen.Values)
            {
                msg += i.id + ";";
            }
            byte[] message = Encoding.Unicode.GetBytes(msg);
            Array.Resize(ref message, 8192);
            tcpClient.GetStream().BeginWrite(message, 0, 8192, null, null);
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }
    }
}
