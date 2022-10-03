using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1(RSAParameters _privKey,RSAParameters _pubKey, bool _isAdmin, TcpClient _client, string _ID)
        {
            InitializeComponent();
            
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            receiveBuffer = new byte[8192];

            client = _client;
            stream = client.GetStream();
            privKey = _privKey;
            pubKey = _pubKey;
            ID = _ID;

            if (_isAdmin)
            {
                Admin.Visible = true;
            }

            byte[] message = Encoding.Unicode.GetBytes("ping;");
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);

            DateTime lastSaveTime =DateTime.MinValue;
            if (File.Exists("saves\\" + ID + "_save.txt")) 
            {
                StreamReader sr = new StreamReader("saves\\" +ID+"_save.txt");
                string key = DecryptRSA(sr.ReadLine(),privKey);
                lastSaveTime=DateTime.Parse(DecryptAES(sr.ReadLine(),key));
                string line;
                string[] lineSplit;
                while (!sr.EndOfStream)//groups loop
                {
                    line = DecryptAES(sr.ReadLine(),key);
                    lineSplit = line.Split(';');
                    Groups g = initGroups(lineSplit[1],lineSplit[4], bool.Parse(lineSplit[0]), DateTime.Parse(lineSplit[3]), 
                        new List<Message>(), 1000, lineSplit[2]);
                    groups.Add(lineSplit[2], g);
                    moveGroupToTop(1000);
                    string groupID = lineSplit[2];
                    line = sr.ReadLine();
                    while (true)
                    {
                        if (line == "")
                        {
                            break;
                        }
                        else
                        {
                            line= DecryptAES(line, key);
                        }
                        lineSplit=line.Split(';');

                        Message m = new Message(lineSplit[0], bool.Parse(lineSplit[3]), bool.Parse(lineSplit[4]), 
                            DateTime.Parse(lineSplit[5]), lineSplit[1], lineSplit[2]);
                        if (m.isImage)
                        {
                            byte[] byteFile = Convert.FromBase64String(lineSplit[6]);
                            m.setImage(Image.FromStream(new MemoryStream(byteFile)));
                        }
                        groups[groupID].newMessage(m);
                        line = sr.ReadLine();
                    }

                }
                sr.Close();
            }
            stream.BeginRead(receiveBuffer, 0, 8192, ReceiveCallback, null);
            message = Encoding.Unicode.GetBytes("ping;");
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);

            message = Encoding.Unicode.GetBytes("load;" + lastSaveTime.ToString() + ";");
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);
        }

        static Random rand = new Random();
        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        static string RSAParameterToString(RSAParameters param)
        {
            string KeyString;
            {
                var sw = new System.IO.StringWriter();
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                xs.Serialize(sw, param);
                KeyString = sw.ToString();
            }
            return KeyString;
        }
        static RSAParameters StringToRSAParameter(string param)
        {
            RSAParameters Key;
            {
                var sr = new System.IO.StringReader(param);
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                Key = (RSAParameters)xs.Deserialize(sr);
            }
            return Key;
        }
        static string EncryptRSA(string plaintext, RSAParameters key)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            csp.ImportParameters(key);
            return Convert.ToBase64String(csp.Encrypt(StringToByte(plaintext), false));

        }
        static string DecryptRSA(string plaintext, RSAParameters key)
        {
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            csp.ImportParameters(key);
            return ByteToString(csp.Decrypt(Convert.FromBase64String(plaintext), false));
        }
        static string GenerateRandomString(int length)
        {
            byte[] bytes = new byte[length * 4 / 8 + 1];
            rand.NextBytes(bytes);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length / 2; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            if (builder.Length < length)
            {
                builder.Append(bytes[bytes.Length - 1].ToString("x2")[0]);
            }
            return builder.ToString();
        }

        static string EncryptAES(string plainText, string key)
        {
            byte[] plainTextByte = StringToByte(plainText);
            byte[] encrypted;

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = StringToByte(key.Substring(0, 16));
                aesAlg.IV = StringToByte(key.Substring(16, 8));

                ICryptoTransform encryptor = aesAlg.CreateEncryptor();
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainTextByte, 0, plainTextByte.Length);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return Convert.ToBase64String(encrypted);
        }
        static string DecryptAES(string encryptedText, string key)
        {
            byte[] cipherText = Convert.FromBase64String(encryptedText);
            byte[] plaintext;
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = StringToByte(key.Substring(0, 16));
                aesAlg.IV = StringToByte(key.Substring(16, 8));



                ICryptoTransform decryptor = aesAlg.CreateDecryptor();
                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(cipherText, 0, cipherText.Length);
                    }
                    plaintext = msDecrypt.ToArray();
                }
            }
            return ByteToString(plaintext);
        }
        static byte[] StringToByte(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            return bytes;
        }
        static string ByteToString(byte[] bytes)
        {
            string text = Encoding.Unicode.GetString(bytes);
            return text;
        }
        static int Max(params int[] values)
        {
            return Enumerable.Max(values);
        }

        class Message
        {
            public string message;
            public Image image;
            public bool isFile;
            public bool isImage;
            
            public DateTime timeSent;
            public string senderID;
            public string senderName;

            public Message(string _message, bool _isFile, bool _isImage, DateTime _timeSent, string _senderID, string _senderName)
            {
                message = _message;
                isFile = _isFile;
                isImage = _isImage;
                timeSent = _timeSent;
                senderID = _senderID;
                senderName = _senderName;
            }
            public void setImage(Image _image)
            {
                image = _image;
            }
        }

        class Groups
        {
            public string name;
            public string division;
            public string GroupID;
            public bool isGroup;
            public DateTime lastTimeSent;
            public List<Message> messages;
            public Panel ChatBox;
            public int pos;

            public Groups(string _name, string _division, bool _isGroup, DateTime _lastTimeSent, List<Message> _messages, int _pos, string _GroupID)
            {
                this.name = _name;
                this.division = _division;
                this.isGroup = _isGroup;
                this.lastTimeSent = _lastTimeSent;
                this.messages = _messages;
                this.GroupID = _GroupID;
                this.pos = _pos;
                
                ChatBox = new Panel();
                ChatBox.Location = new Point(0, 60*pos);
                ChatBox.Size = new Size(219, 60);
                ChatBox.BackColor = Color.White;
                ChatBox.BorderStyle=BorderStyle.FixedSingle;
                ChatBox.Tag = this;
                updateTexts();
            }
            private void updateTexts()
            {
                clearControllSafe(ChatBox);
                Label n = new Label();
                if (!isGroup)
                {
                    n.Text = name+"   "+division;
                }
                else
                {
                    n.Text = name;
                }
                n.Location = new Point(10, 0);
                n.Font = new Font("Microsoft Sans Serif", 10);
                n.Visible = true;
                addControllSafe<Label>(n,ChatBox);
                Label m = new Label();
                if (messages.Count > 0)
                {
                    if (messages.Last().isImage)
                    {
                        m.Text = "Image";
                    }
                    else if (messages.Last().isFile)
                    {
                        m.Text = "File";
                    }
                    else
                    {
                        m.Text = messages.Last().message;
                    }
                }
                m.Location = new Point(10, 5 + n.Height);
                m.Font = new Font("Microsoft Sans Serif", 8);
                m.Visible = true;
                addControllSafe<Label>(m, ChatBox);

                Label t = new Label();
                if (lastTimeSent.CompareTo(DateTime.Today.AddDays(-1)) < 0)
                {
                    t.Text = lastTimeSent.ToString("dd-MM-yy HH:mm");
                }
                else
                {
                    t.Text = lastTimeSent.ToString("HH:mm");
                }
                t.Location = new Point(209 - t.Width, 0);
                t.Font = new Font("Microsoft Sans Serif", 8);
                t.Visible = true;
                addControllSafe<Label>(t, ChatBox);
            }
            public void newMessage(Message msg)
            {
                messages.Add(msg);
                lastTimeSent = DateTime.Now;
                updateTexts();
            }
            private delegate void SafeCallDelegateMove(Point p, Panel panel);
            public void movePanelSafe(Point p, Panel panel)
            {
                if (panel.InvokeRequired)
                {
                    var d = new SafeCallDelegateMove(movePanelSafe);
                    panel.Invoke(d, new object[] { p, panel });
                }
                else
                {
                    panel.Location = p;
                }
            }
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
            public void changePos(int newPos)
            {
                this.pos = newPos;
                movePanelSafe(new Point(0, 60 * pos), ChatBox);
            }
        }

        class ShownMessages
        {
            List<Message> m = new List<Message>();
            public List<Panel> messageBox = new List<Panel>();
            int lastLoc = 0;
            string ID;

            public EventHandler FileClicked;

            protected virtual void OnFileClicked(object sender, EventArgs args)
            {
                EventHandler handler = FileClicked;
                string fileCode="";
                if(sender is Panel)
                {
                    fileCode= ((Panel)sender).Tag as string;
                }
                else if (sender is Label)
                {
                    fileCode = ((Label)sender).Tag as string;
                }
                else if (sender is PictureBox)
                {
                    fileCode = ((PictureBox)sender).Tag as string;
                }
                if (handler != null)
                {
                    handler(fileCode, EventArgs.Empty);
                }
            }
            
            public ShownMessages(List<Message> _m, string _ID)
            {
                this.ID = _ID;
                for (int i = 0; i < _m.Count; i++)
                {
                    newMessage(_m[i],0);
                }
            }
            private Size getSize(Size imageSize, int maximumX)
            {
                if (maximumX > imageSize.Width)
                {
                    return imageSize;
                }
                else
                {
                    float ratio=(float)maximumX/(float)imageSize.Width;
                    return new Size((int)(imageSize.Width * ratio), (int)(imageSize.Height * ratio));              
                }
            }
            public void newMessage(Message msg,int scrollValue)
            {
                if (scrollValue == 91)
                {
                    scrollValue = 0;
                }

                Panel p = new Panel();
                p.Tag = msg;

                Label timeLabel = new Label();
                timeLabel.MaximumSize = new Size(330, 0);
                timeLabel.AutoSize = true;
                if (msg.timeSent.CompareTo(DateTime.Today.AddDays(-1)) < 0)
                {
                    timeLabel.Text = msg.timeSent.ToString("dd-MM-yy HH:mm");
                }
                else
                {
                    timeLabel.Text = msg.timeSent.ToString("HH:mm");
                }

                bool showName = true;
                Label nameLabel = new Label();
                if (messageBox.Count > 0)
                {
                    if (msg.senderID == m.Last().senderID)
                    {
                        showName = false;
                    }
                }
                if (!msg.isFile)
                {
                    Label messageLabel = new Label();
                    messageLabel.MaximumSize = new Size(330, 0);
                    messageLabel.AutoSize = true;
                    messageLabel.Text = msg.message;


                    if (showName)
                    {
                        nameLabel.MaximumSize = new Size(330, 0);
                        nameLabel.AutoSize = true;
                        nameLabel.Text = msg.senderName;

                        p.Controls.Add(nameLabel);
                        p.Controls.Add(timeLabel);
                        p.Controls.Add(messageLabel);


                        p.Size = new Size(Max(messageLabel.Width, timeLabel.Width, nameLabel.Width) + 30, messageLabel.Height + timeLabel.Height + nameLabel.Height + 30);

                        if (ID == msg.senderID)
                        {
                            nameLabel.Location = new Point(10, 10);
                            messageLabel.Location = new Point(10, nameLabel.Height + 15);
                            timeLabel.Location = new Point(10, nameLabel.Height + messageLabel.Height + 20);
                            p.BackColor = Color.LightGreen;
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            nameLabel.Location = new Point(10, 10);
                            messageLabel.Location = new Point(10, nameLabel.Height + 15);
                            timeLabel.Location = new Point(10, nameLabel.Height + messageLabel.Height + 20);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                    }
                    else
                    {
                        p.Controls.Add(timeLabel);
                        p.Controls.Add(messageLabel);

                        p.Size = new Size(Max(messageLabel.Width, timeLabel.Width) + 30, messageLabel.Height + timeLabel.Height + 25);
                        if (ID == msg.senderID)
                        {
                            messageLabel.Location = new Point(10, 10);
                            timeLabel.Location = new Point(10, messageLabel.Height + 15);
                            p.BackColor = Color.LightGreen;
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            messageLabel.Location = new Point(10, 10);
                            timeLabel.Location = new Point(10, messageLabel.Height + 15);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                    }
                }
                else if (msg.isImage)
                {
                    PictureBox picture = new PictureBox();
                    picture.Size = getSize(msg.image.Size, 330);
                    picture.SizeMode = PictureBoxSizeMode.Zoom;
                    picture.Image = msg.image;
                    picture.Visible = true;

                    if (showName)
                    {
                        nameLabel.MaximumSize = new Size(330, 0);
                        nameLabel.AutoSize = true;
                        nameLabel.Text = msg.senderName;

                        p.Controls.Add(nameLabel);
                        p.Controls.Add(timeLabel);
                        p.Controls.Add(picture);


                        p.Size = new Size(Max(picture.Width, timeLabel.Width, nameLabel.Width) + 30, picture.Height + timeLabel.Height + nameLabel.Height + 30);

                        if (ID == msg.senderID)
                        {
                            nameLabel.Location = new Point(10, 10);
                            picture.Location = new Point(10, nameLabel.Height + 15);
                            timeLabel.Location = new Point(10, nameLabel.Height + picture.Height + 20);
                            p.BackColor = Color.LightGreen;
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            nameLabel.Location = new Point(10, 10);
                            picture.Location = new Point(10, nameLabel.Height + 15);
                            timeLabel.Location = new Point(10, nameLabel.Height + picture.Height + 20);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                    }
                    else
                    {
                        p.Controls.Add(timeLabel);
                        p.Controls.Add(picture);

                        p.Size = new Size(Max(picture.Width, timeLabel.Width) + 30, picture.Height + timeLabel.Height + 25);
                        if (ID == msg.senderID)
                        {
                            picture.Location = new Point(10, 10);
                            timeLabel.Location = new Point(10, picture.Height + 15);
                            p.BackColor = Color.LightGreen;
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            picture.Location = new Point(10, 10);
                            timeLabel.Location = new Point(10, picture.Height + 15);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                    }
                    picture.Update();
                }
                else
                {
                    p.Click += OnFileClicked;
                    PictureBox picture = new PictureBox();
                    picture.Image = Image.FromFile("file-icon.jpg");
                    picture.Size = getSize(picture.Image.Size, 50);
                    picture.SizeMode = PictureBoxSizeMode.Zoom;
                    picture.Visible = true;
                    picture.Location=new Point(10, 10);
                    picture.Tag = msg.message;
                    picture.Click += OnFileClicked;

                    Label messageLabel = new Label();
                    messageLabel.MaximumSize = new Size(270, 0);
                    messageLabel.AutoSize = true;
                    messageLabel.Text = String.Join(".",msg.message.Split('.').Skip(1));
                    messageLabel.Tag = msg.message;
                    messageLabel.Click += OnFileClicked;

                    timeLabel.Tag=msg.message;
                    timeLabel.MaximumSize = new Size(270, 0);
                    timeLabel.Click += OnFileClicked;

                    p.Tag=msg.message;
                    if(showName)
                    {
                        nameLabel.MaximumSize = new Size(2700, 0);
                        nameLabel.AutoSize = true;
                        nameLabel.Text = msg.senderName;
                        nameLabel.Tag = msg.message;
                        nameLabel.Click += OnFileClicked;

                        p.Controls.Add(nameLabel);
                        p.Controls.Add(timeLabel);
                        p.Controls.Add(messageLabel);
                        p.Controls.Add(picture);


                        p.Size = new Size(Max(messageLabel.Width, timeLabel.Width, nameLabel.Width) + picture.Width + 40,Math.Max(messageLabel.Height + timeLabel.Height + nameLabel.Height + 30,picture.Height+10));

                        if (ID == msg.senderID)
                        {
                            nameLabel.Location = new Point(20+picture.Width, 10);
                            messageLabel.Location = new Point(20 + picture.Width, nameLabel.Height + 15);
                            timeLabel.Location = new Point(20 + picture.Width, nameLabel.Height + messageLabel.Height + 20);
                            p.BackColor = Color.LightGreen;
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            nameLabel.Location = new Point(20 + picture.Width, 10);
                            messageLabel.Location = new Point(20 + picture.Width, nameLabel.Height + 15);
                            timeLabel.Location = new Point(20 + picture.Width, nameLabel.Height + messageLabel.Height + 20);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                    }
                    else
                    {
                        p.Controls.Add(timeLabel);
                        p.Controls.Add(picture);
                        p.Controls.Add(messageLabel);

                        p.Size = new Size(Max(messageLabel.Width, timeLabel.Width) + picture.Width + 40, Math.Max(messageLabel.Height + timeLabel.Height + 30, picture.Height+20));
                        if (ID == msg.senderID)
                        {
                            messageLabel.Location = new Point(20 + picture.Width, 10);
                            timeLabel.Location = new Point(20 + picture.Width, messageLabel.Height + 15);
                            p.BackColor = Color.LightGreen;
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            messageLabel.Location = new Point(20 + picture.Width, 10);
                            timeLabel.Location = new Point(20 + picture.Width, messageLabel.Height + 15);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5 - scrollValue);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                    }
                    picture.Update();
                }
                messageBox.Add(p);
                m.Add(msg);
            }
        }
        private void moveGroupToTop(int pos)
        {
            foreach(Groups i in groups.Values)
            {
                if (i.pos < pos)
                {
                    i.changePos(i.pos + 1);
                }
                else if (i.pos == pos)
                {
                    i.changePos(0);
                }
            }
        }
        Groups initGroups(string _name, string division, bool _isGroup, DateTime _lastTimeSent, List<Message> _messages, int pos, string _GroupID)
        {
            Groups g = new Groups(_name, division, _isGroup, _lastTimeSent, _messages, pos, _GroupID);
            addControllSafe<Panel>(g.ChatBox, GroupsBox);
            g.ChatBox.Click += ClickGroups;
            return g;
        }
        private void ClickGroups(object sender, EventArgs e)
        {
            Groups s = (Groups)((Panel)sender).Tag;
            label1.Text = s.name + " " + s.division;
            clearControllSafe(ChatBox);
            ChatBox.VerticalScroll.Value = 0;
            ChatBox.PerformLayout();
            shownMessages = new ShownMessages(s.messages, ID);
            shownMessages.FileClicked += downloadFile;
            foreach(Panel p in shownMessages.messageBox)
            {
                addControllSafe<Panel>(p,ChatBox);
            }
            ChatBox.PerformLayout();
            maxScrollSafe(ChatBox);

            currentGroupID = s.GroupID;
        }
        private void downloadFile(object code, EventArgs e)
        {
            string c=(string)code;
            byte[] message = Encoding.Unicode.GetBytes("downloadfile;"+code+";");
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);
        }
        private delegate void SafeCallDelegate(object obj, Panel panel);
        private delegate void SafeCallDelegatePanel(Panel panel);
        private delegate void SafeCallDelegateChangeText(TextBox textBox, string text);

        public void changeTextSafe(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                var d = new SafeCallDelegateChangeText(changeTextSafe);
                textBox.Invoke(d, new object[] { textBox, text });
            }
            else
            {
                textBox.Text=text;
            }
        }
        
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

        public void maxScrollSafe(Panel panel)
        {
            if (panel.InvokeRequired)
            {
                var d = new SafeCallDelegatePanel(maxScrollSafe);
                panel.Invoke(d, new object[] {panel});
            }
            else
            {
                panel.PerformLayout();
                panel.VerticalScroll.Value = panel.VerticalScroll.Maximum + 1 - panel.VerticalScroll.LargeChange;
                
                if (panel.VerticalScroll.Visible == false)
                {
                    panel.VerticalScroll.Value = 0;
                }
                panel.PerformLayout();
            }
        }

        Int32 port = 13000;
        TcpClient client;
        NetworkStream stream;
        private byte[] receiveBuffer;
        Dictionary<string, Groups> groups = new Dictionary<string, Groups>();
        ShownMessages shownMessages;
        RSAParameters privKey;
        RSAParameters pubKey;
        string ID;
        string currentGroupID;
        Dictionary<string,string> file = new Dictionary<string,string>();
        Form4 form4;
        Form5 form5;
        bool moving=false;

        private void Form1_Load(object sender, EventArgs e)
        {
            receiveBuffer = new byte[8192];
            ChatBox.AutoScroll = false;
            ChatBox.HorizontalScroll.Enabled = false;
            ChatBox.HorizontalScroll.Visible = false;
            ChatBox.HorizontalScroll.Maximum = 0;
            ChatBox.AutoScroll = true;
            form4 = new Form4(client, ID);
            form4.Visible = false;
            form5 = new Form5(client, ID);
            form5.Visible = false;

        }
        
        /*private void changeText(string _msg)
        {
            if (textBox2.InvokeRequired)
            {
                var d = new SafeCallDelegate(changeText);
                textBox2.Invoke(d, new object[] { _msg });
            }
            else
            {
                textBox2.Text = _msg;
            }
        }*/
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (!moving)
                {
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    string dataString = Encoding.Unicode.GetString(_data);
                    string[] dataSplit = dataString.Split(';');
                    if (dataSplit[0] == "msg")
                    {
                        string key = DecryptRSA(dataSplit[1], privKey);
                        string msgString = DecryptAES(dataSplit[5], key);
                        Message msg = new Message(msgString, false, false, DateTime.Now, dataSplit[3], dataSplit[4]);
                        string msgGroupId;
                        if (dataSplit[2] == ID)
                        {
                            msgGroupId = dataSplit[3];
                        }
                        else
                        {
                            msgGroupId = dataSplit[2];
                        }
                        groups[msgGroupId].newMessage(msg);
                        if (msgGroupId == currentGroupID)
                        {
                            bool scrollDown = false;
                            //MessageBox.Show(ChatBox.VerticalScroll.Value.ToString()+" "+ (ChatBox.VerticalScroll.Maximum + 1 - ChatBox.VerticalScroll.LargeChange).ToString());
                            if (ChatBox.VerticalScroll.Value >= ChatBox.VerticalScroll.Maximum + 1 - ChatBox.VerticalScroll.LargeChange)
                            {
                                scrollDown = true;
                            }
                            shownMessages.newMessage(msg, ChatBox.VerticalScroll.Value);
                            addControllSafe<Panel>(shownMessages.messageBox.Last(), ChatBox);
                            if (scrollDown)
                            {
                                maxScrollSafe(ChatBox);
                            }
                        }
                        int posGroup = 0;
                        foreach (Groups i in groups.Values)
                        {
                            if (i.GroupID == msgGroupId)
                            {
                                posGroup = i.pos;
                                break;
                            }
                        }
                        moveGroupToTop(posGroup);
                    }
                    else if (dataSplit[0] == "filemsg")
                    {
                        string key = DecryptRSA(dataSplit[1], privKey);
                        Message msg = new Message(dataSplit[5], true, false, DateTime.Now, dataSplit[3], dataSplit[4]);
                        string msgGroupId;
                        if (dataSplit[2] == ID)
                        {
                            msgGroupId = dataSplit[3];
                        }
                        else
                        {
                            msgGroupId = dataSplit[2];
                        }
                        groups[msgGroupId].newMessage(msg);
                        if (msgGroupId == currentGroupID)
                        {
                            bool scrollDown = false;
                            if (ChatBox.VerticalScroll.Value >= ChatBox.VerticalScroll.Maximum + 1 - ChatBox.VerticalScroll.LargeChange)
                            {
                                scrollDown = true;
                            }
                            shownMessages.newMessage(msg, ChatBox.VerticalScroll.Value);
                            addControllSafe<Panel>(shownMessages.messageBox.Last(), ChatBox);
                            if (scrollDown)
                            {
                                maxScrollSafe(ChatBox);
                            }
                        }
                        int posGroup = 0;
                        foreach (Groups i in groups.Values)
                        {
                            if (i.GroupID == msgGroupId)
                            {
                                posGroup = i.pos;
                                break;
                            }
                        }
                        moveGroupToTop(posGroup);
                    }
                    else if (dataSplit[0] == "imgmsg")
                    {
                        if (dataSplit[1] == "false")
                        {
                            if (!file.ContainsKey(dataSplit[2]))
                            {
                                file.Add(dataSplit[2], "");
                            }
                            file[dataSplit[2]] += dataSplit[3];
                        }
                        else
                        {
                            string key = DecryptRSA(dataSplit[2], privKey);
                            string decryptedFile = DecryptAES(file[dataSplit[6]], key);
                            byte[] byteFile = Convert.FromBase64String(decryptedFile);
                            file.Remove(dataSplit[6]);
                            Message msg = new Message(dataSplit[6], true, true, DateTime.Now, dataSplit[4], dataSplit[5]);
                            msg.setImage(Image.FromStream(new MemoryStream(byteFile)));
                            string msgGroupId;
                            if (dataSplit[3] == ID)
                            {
                                msgGroupId = dataSplit[4];
                            }
                            else
                            {
                                msgGroupId = dataSplit[3];
                            }
                            groups[msgGroupId].newMessage(msg);
                            if (msgGroupId == currentGroupID)
                            {
                                bool scrollDown = false;
                                if (ChatBox.VerticalScroll.Value >= ChatBox.VerticalScroll.Maximum + 1 - ChatBox.VerticalScroll.LargeChange)
                                {
                                    scrollDown = true;
                                }
                                shownMessages.newMessage(msg, ChatBox.VerticalScroll.Value);
                                addControllSafe<Panel>(shownMessages.messageBox.Last(), ChatBox);
                                if (scrollDown)
                                {
                                    maxScrollSafe(ChatBox);
                                }
                            }
                            int posGroup = 0;
                            foreach (Groups i in groups.Values)
                            {
                                if (i.GroupID == msgGroupId)
                                {
                                    posGroup = i.pos;
                                    break;
                                }
                            }
                            moveGroupToTop(posGroup);
                        }
                    }
                    else if (dataSplit[0] == "downloadfile")
                    {
                        if (dataSplit[1] == "false")
                        {
                            if (!file.ContainsKey(dataSplit[2]))
                            {
                                file.Add(dataSplit[2], "");
                            }
                            file[dataSplit[2]] += dataSplit[3];
                        }
                        else
                        {
                            string key = DecryptRSA(dataSplit[3], privKey);
                            string decryptedFile = DecryptAES(file[dataSplit[2]], key);
                            byte[] byteFile = Convert.FromBase64String(decryptedFile);
                            file.Remove(dataSplit[2]);
                            string filepath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + Path.GetFileNameWithoutExtension(String.Join(".", dataSplit[2].Split('.').Skip(1)));
                            string extension = Path.GetExtension(String.Join(".", dataSplit[2].Split('.').Skip(1)));
                            if (File.Exists(filepath + extension))
                            {
                                int i = 0;
                                while (true)
                                {
                                    i++;
                                    if (!File.Exists(filepath + "(" + i.ToString() + ")" + extension))
                                    {
                                        filepath = filepath + "(" + i.ToString() + ")" + extension;
                                        break;
                                    }
                                }
                            }
                            File.WriteAllBytes(filepath, byteFile);
                        }
                    }
                    else if (dataSplit[0] == "load")
                    {
                        if (dataSplit[1] == "false")
                        {
                            if (!file.ContainsKey("load"))
                            {
                                file.Add("load", String.Join(";", dataSplit.Skip(2).Take(dataSplit.Length - 3)));
                            }
                            else
                            {
                                file["load"] += String.Join(";", dataSplit.Skip(2).Take(dataSplit.Length - 3));
                            }
                        }
                        else if (file.ContainsKey("load"))
                        {
                            string[] result = file["load"].Split('\n');
                            file.Remove("load");
                            foreach (string line in result)
                            {
                                string[] lineSplit = line.Split(';');
                                if (lineSplit[0] == "group")
                                {
                                    if (!groups.ContainsKey(lineSplit[2]))
                                    {
                                        if (lineSplit[1] == "false")
                                        {
                                            Groups g = initGroups(lineSplit[3], lineSplit[4], false, DateTime.MinValue, new List<Message>(), 1000, lineSplit[2]);
                                            groups.Add(lineSplit[2], g);
                                            moveGroupToTop(1000);
                                        }
                                        else
                                        {
                                            Groups g = initGroups(lineSplit[3], null, true, DateTime.MinValue, new List<Message>(), 1000, lineSplit[2]);
                                            groups.Add(lineSplit[2], g);
                                            moveGroupToTop(1000);
                                        }
                                    }
                                }
                                else if (lineSplit[0] != "")
                                {
                                    string msg = lineSplit[4];
                                    string key = DecryptRSA(lineSplit[8], privKey);
                                    if (!bool.Parse(lineSplit[6]))
                                    {
                                        msg = DecryptAES(msg, key);
                                    }
                                    Message m = new Message(msg, bool.Parse(lineSplit[6]), bool.Parse(lineSplit[7]), DateTime.Parse(lineSplit[5]), lineSplit[1], lineSplit[9]);
                                    if (m.isImage)
                                    {
                                        byte[] byteFile = Convert.FromBase64String(DecryptAES(lineSplit[10], key));
                                        m.setImage(Image.FromStream(new MemoryStream(byteFile)));
                                    }
                                    string groupID = lineSplit[2];
                                    if (groupID == ID)
                                    {
                                        groupID = lineSplit[1];
                                    }
                                    groups[groupID].newMessage(m);
                                }
                            }
                        }
                    }
                    else if (dataSplit[0] == "msg2")
                    {
                        string key = DecryptRSA(dataSplit[1], privKey);
                        string encryptedMsg = EncryptAES(msgTextBox.Text, key);
                        changeTextSafe(msgTextBox, "");
                        byte[] message = Encoding.Unicode.GetBytes("msgSend;" + currentGroupID + ";" + groups[currentGroupID].isGroup.ToString().ToLower() + ";false;" + DateTime.Now.ToString() + ";" + encryptedMsg + ";");
                        Array.Resize(ref message, 8192);
                        stream.BeginWrite(message, 0, 8192, null, null);
                    }
                    else if (dataSplit[0] == "filemsg2")
                    {
                        string key = DecryptRSA(dataSplit[1], privKey);
                        Console.WriteLine(key);
                        string filename = msgTextBox.Text.Split('\\').Last();
                        string isImage = msgTextBox.Text.Split(';')[0].ToLower();
                        byte[] fileBytes;
                        using (FileStream fs = new FileStream(msgTextBox.Text.Split(';')[1], FileMode.Open, FileAccess.Read))
                        {
                            fileBytes = File.ReadAllBytes(msgTextBox.Text.Split(';')[1]);
                            fs.Read(fileBytes, 0, Convert.ToInt32(fs.Length));
                            fs.Close();
                        }

                        string encryptedFile = EncryptAES(Convert.ToBase64String(fileBytes), key);
                        Console.WriteLine(encryptedFile);
                        int numBatch = (encryptedFile.Length + 3999) / 4000;
                        byte[] message;
                        for (int i = 0; i < numBatch; i++)
                        {
                            message = Encoding.Unicode.GetBytes("fileSend;false;" + encryptedFile.Substring(i * 4000, Math.Min(4000, encryptedFile.Length - i * 4000)) + ";");
                            Array.Resize(ref message, 8192);
                            stream.BeginWrite(message, 0, 8192, null, null);
                        }
                        message = Encoding.Unicode.GetBytes("fileSend;true;" + currentGroupID + ";" + groups[currentGroupID].isGroup.ToString().ToLower() + ";true;" + isImage + ";" + DateTime.Now.ToString() + ";" + filename + ";");
                        Array.Resize(ref message, 8192);
                        stream.BeginWrite(message, 0, 8192, null, null);
                        changeTextSafe(msgTextBox, "");
                    }
                    else if (dataSplit[0] == "find")
                    {
                        if (dataSplit[1] == "false")
                        {
                            form4.findResult(dataString);
                        }
                        else
                        {
                            form5.findResult(dataString);
                        }
                    }
                    else if (dataSplit[0] == "add")
                    {
                        if (!groups.ContainsKey(dataSplit[1]))
                        {
                            Groups g = initGroups(dataSplit[2], dataSplit[3], false, DateTime.Now, new List<Message>(), 1000, dataSplit[1]);
                            groups.Add(dataSplit[1], g);
                            moveGroupToTop(1000);
                        }
                    }
                    else if (dataSplit[0] == "newGroup")
                    {
                        Groups g = initGroups(dataSplit[2], null, true, DateTime.Now, new List<Message>(), 1000, dataSplit[1]);
                        groups.Add(dataSplit[1], g);
                        moveGroupToTop(1000);
                    }
                    else if (dataSplit[0] == "adminBroadcast")
                    {
                        Message msg = new Message(dataSplit[3], false, false, DateTime.Now, dataSplit[2], dataSplit[1]);
                        if (!groups.ContainsKey("Broadcast"))
                        {
                            groups.Add("Broadcast", initGroups("Admin Broadcast", "", false, DateTime.Now, new List<Message>(), 1000, "Broadcast"));
                            moveGroupToTop(1000);
                        }
                        groups["Broadcast"].newMessage(msg);
                        if ("Broadcast" == currentGroupID)
                        {
                            bool scrollDown = false;
                            if (ChatBox.VerticalScroll.Value >= ChatBox.VerticalScroll.Maximum + 1 - ChatBox.VerticalScroll.LargeChange)
                            {
                                scrollDown = true;
                            }
                            shownMessages.newMessage(msg, ChatBox.VerticalScroll.Value);
                            addControllSafe<Panel>(shownMessages.messageBox.Last(), ChatBox);
                            if (scrollDown)
                            {
                                maxScrollSafe(ChatBox);
                            }
                        }
                    }
                if (!moving)
                    {
                        stream.BeginRead(receiveBuffer, 0, 8192, ReceiveCallback, null);
                    }
                }
            }
            catch 
            {    
                if (!moving) 
                {
                    Disconnect();
                }
            }
            
        }
        public static byte[] ImageToBytes(Image img)
        {
            byte[] result;
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, img.RawFormat);
                result = stream.ToArray();
            }
            return result;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Directory.CreateDirectory("saves");
            StreamWriter sw = new StreamWriter("saves\\"+ID+"_save.txt");
            string key = GenerateRandomString(24);
            string encryptedKey = EncryptRSA(key,pubKey);
            sw.WriteLine(encryptedKey);
            sw.WriteLine(EncryptAES(DateTime.Now.ToString(),key));
            foreach(Groups i in groups.Values)
            {
                string line = i.isGroup.ToString() + ";" + i.name + ";" + i.GroupID + ";" + i.lastTimeSent.ToString() + ";"+i.division+";";
                sw.WriteLine(EncryptAES(line, key));
                foreach(Message m in i.messages)
                {
                    line = m.message + ";" + m.senderID + ";" + m.senderName + ";" + m.isFile.ToString() + ";" + m.isImage.ToString() + ";" 
                        + m.timeSent.ToString() + ";";
                    if (m.isImage){
                        line+=Convert.ToBase64String(ImageToBytes(m.image));
                    }
                    sw.WriteLine(EncryptAES(line, key));
                }
                sw.WriteLine();
            }
            sw.Close();

            if (!moving)
            {
                Disconnect();
            }
            form4.Dispose();
            form5.Dispose();
        }


        private void Disconnect()
        {
            if (client != null)
            {
                Console.WriteLine("Dissconnected");
                byte[] message = Encoding.Unicode.GetBytes("dc;");
                Array.Resize(ref message, 8192);
                stream.BeginWrite(message, 0, 8192, null, null);
                client.Close();
                client = null;
            }
        }

        private void Find_Click(object sender, EventArgs e)
        {
            form4.Visible = true;
        }

        private void Make_Group_Click(object sender, EventArgs e)
        {
            form5.Visible = true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (currentGroupID != null)
            {
                if (e.KeyChar == (char)13 && currentGroupID!="Broadcast")
                {
                    byte[] message = Encoding.Unicode.GetBytes("msg;"+ID+";false;");
                    Array.Resize(ref message, 8192);
                    stream.BeginWrite(message, 0, 8192, null, null);
                }
            }
            else
            {
                msgTextBox.Text = "";
                e.Handled = true;
            }
        }
        
        private void sendFile(bool isImage)
        {
            if (currentGroupID != null)
            {
                string filePath = "";
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    if (isImage)
                    {
                        openFileDialog.Filter = "Image files|*.jpg;*.jpeg;*.png;";
                    }
                    else
                    {
                        openFileDialog.Filter = "All files|*.*";
                    }
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        filePath = openFileDialog.FileName;
                        msgTextBox.Text = isImage.ToString() + ";" + filePath;
                        byte[] message = Encoding.Unicode.GetBytes("msg;" + ID + ";true;");
                        Array.Resize(ref message, 8192);
                        stream.BeginWrite(message, 0, 8192, null, null);
                    }
                }
            }
        }
        private void FileButton_Click(object sender, EventArgs e)
        {
            sendFile(false);
        }

        private void Imagebutton_Click(object sender, EventArgs e)
        {
            sendFile(true);
        }

        private void ChatBox_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ChatBox_ControlAdded(object sender, ControlEventArgs e)
        {

        }
        private delegate void SafeCallDelegateChangeForm(RSAParameters privKey, RSAParameters pubKey);

        private void changeForm(RSAParameters privKey, RSAParameters pubKey)
        {
            if (this.InvokeRequired)
            {
                var d = new SafeCallDelegateChangeForm(changeForm);
                this.Invoke(d, new object[] { privKey, pubKey});
            }
            else
            {
                Form6 form = new Form6(privKey, pubKey, true, client, ID);
                form.Show();
                this.Close();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            moving = true;
            changeForm(privKey, pubKey);
        }

        private void GroupsBox_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
