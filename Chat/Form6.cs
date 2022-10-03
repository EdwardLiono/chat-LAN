using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using System.Threading;

namespace Test
{
    public partial class Form6 : Form
    {
        public Form6(RSAParameters _privKey, RSAParameters _pubKey, bool _isAdmin, TcpClient _client, string _ID)
        {
            InitializeComponent();

            receiveBuffer = new byte[8192];

            this.MaximizeBox = false;
            this.MinimizeBox = false;

            client = _client;
            stream = client.GetStream();
            privKey = _privKey;
            pubKey = _pubKey;
            ID = _ID;

            stream.BeginRead(receiveBuffer, 0, 8192, ReceiveCallback, null);
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
        Dictionary<string, string> file = new Dictionary<string, string>();
        bool moving=false;
        Form7 form7;

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
                ChatBox.Location = new Point(0, 60 * pos);
                ChatBox.Size = new Size(219, 60);
                ChatBox.BackColor = Color.White;
                ChatBox.BorderStyle = BorderStyle.FixedSingle;
                ChatBox.Tag = this;
                updateTexts();
            }
            private void updateTexts()
            {
                clearControllSafe(ChatBox);
                Label n = new Label();
                if (!isGroup)
                {
                    n.Text = name + "   " + division;
                }
                else
                {
                    n.Text = name;
                }
                n.Location = new Point(10, 0);
                n.Font = new Font("Microsoft Sans Serif", 10);
                n.Visible = true;
                addControllSafe<Label>(n, ChatBox);
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

                /*Label t = new Label();
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
                addControllSafe<Label>(t, ChatBox);*/
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
                string fileCode = "";
                if (sender is Panel)
                {
                    fileCode = ((Panel)sender).Tag as string;
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
                    newMessage(_m[i], 0);
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
                    float ratio = (float)maximumX / (float)imageSize.Width;
                    return new Size((int)(imageSize.Width * ratio), (int)(imageSize.Height * ratio));
                }
            }
            public void newMessage(Message msg, int scrollValue)
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
                            p.Location = new Point(381 - p.Width - 20, lastLoc);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            nameLabel.Location = new Point(10, 10);
                            picture.Location = new Point(10, nameLabel.Height + 15);
                            timeLabel.Location = new Point(10, nameLabel.Height + picture.Height + 20);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5);
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
                            p.Location = new Point(381 - p.Width - 20, lastLoc + 5);
                            lastLoc = lastLoc + 5 + p.Height;
                        }
                        else
                        {
                            picture.Location = new Point(10, 10);
                            timeLabel.Location = new Point(10, picture.Height + 15);
                            p.BackColor = Color.White;
                            p.Location = new Point(20, lastLoc + 5);
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
                    picture.Location = new Point(10, 10);
                    picture.Tag = msg.message;
                    picture.Click += OnFileClicked;

                    Label messageLabel = new Label();
                    messageLabel.MaximumSize = new Size(270, 0);
                    messageLabel.AutoSize = true;
                    messageLabel.Text = String.Join(".", msg.message.Split('.').Skip(1));
                    messageLabel.Tag = msg.message;
                    messageLabel.Click += OnFileClicked;

                    timeLabel.Tag = msg.message;
                    timeLabel.MaximumSize = new Size(270, 0);
                    timeLabel.Click += OnFileClicked;

                    if (showName)
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


                        p.Size = new Size(Max(messageLabel.Width, timeLabel.Width, nameLabel.Width) + picture.Width + 40, Math.Max(messageLabel.Height + timeLabel.Height + nameLabel.Height + 30, picture.Height));

                        if (ID == msg.senderID)
                        {
                            nameLabel.Location = new Point(20 + picture.Width, 10);
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

                        p.Size = new Size(Max(messageLabel.Width, timeLabel.Width) + picture.Width + 40, Math.Max(messageLabel.Height + timeLabel.Height + 30, picture.Height));
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
            foreach (Groups i in groups.Values)
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
            byte[] message = Encoding.Unicode.GetBytes("adminMessageLoad;" + s.GroupID + ";" + s.lastTimeSent +";") ;
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);
        }
        private void downloadFile(object code, EventArgs e)
        {
            string c = (string)code;
            byte[] message = Encoding.Unicode.GetBytes("downloadfile;" + code + ";");
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
                textBox.Text = text;
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
                panel.Invoke(d, new object[] { panel });
            }
            else
            {
                panel.VerticalScroll.Value = ChatBox.VerticalScroll.Maximum + 1 - ChatBox.VerticalScroll.LargeChange;
                panel.PerformLayout();
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            //try
            //{
                int _byteLength = stream.EndRead(_result);

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                string dataString = Encoding.Unicode.GetString(_data);
                string[] dataSplit = dataString.Split(';');
                if (dataSplit[0] == "adminSearch")
                {
                    if (dataSplit[1] == "false")
                    {
                        if (!file.ContainsKey("adminSearch"))
                        {
                            file.Add("adminSearch", "");
                        }
                        file["adminSearch"] += String.Join(";", dataSplit.Skip(2).Take(dataSplit.Length - 3));
                    }
                    else if(file.ContainsKey("adminSearch"))
                    {
                        string[] result = file["adminSearch"].Split('\n');
                        file.Remove("adminSearch");
                        foreach (string line in result)
                        {
                            string[] lineSplit = line.Split(';');
                            
                            if (lineSplit[0]!="" && !groups.ContainsKey(lineSplit[2]))
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
                    }
                }
                else if(dataSplit[0] == "adminMessageLoad")
                {
                    if (dataSplit[1] == "false")
                    {
                        if (!file.ContainsKey("adminMessageLoad"))
                        {
                            file.Add("adminMessageLoad", "");
                        }
                        file["adminMessageLoad"] += String.Join(";", dataSplit.Skip(2).Take(dataSplit.Length - 3));
                    }
                    else if (file.ContainsKey("adminMessageLoad"))
                    {
                        string[] result = file["adminMessageLoad"].Split('\n');
                        file.Remove("adminMessageLoad");
                        foreach (string line in result)
                        {
                            string[] lineSplit = line.Split(';');
                            if (lineSplit[0] != "")
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
                                string groupID = dataSplit[2];
                                groups[groupID].newMessage(m);
                            }
                        }
                        Groups s = groups[dataSplit[2]];
                    
                        clearControllSafe(ChatBox);
                        ChatBox.VerticalScroll.Value = 0;
                        ChatBox.PerformLayout();
                        shownMessages = new ShownMessages(s.messages, UserIDSearch.Text);
                        shownMessages.FileClicked += downloadFile;
                        foreach (Panel p in shownMessages.messageBox)
                        {
                            addControllSafe<Panel>(p, ChatBox);
                        }
                        maxScrollSafe(ChatBox);

                        currentGroupID = s.GroupID;
                    }
                }
                if (moving == false)
                {
                    stream.BeginRead(receiveBuffer, 0, 8192, ReceiveCallback, null);
                }
            //}
            //catch
            //{
            //   Disconnect();
            //}
        }
        private void Disconnect()
        {
            if (client != null)
            {
                Console.WriteLine("Disconnected");
                byte[] message = Encoding.Unicode.GetBytes("dc;");
                Array.Resize(ref message, 8192);
                stream.BeginWrite(message, 0, 8192, null, null);
                client.Close();
                client = null;
            }
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            receiveBuffer = new byte[8192];
            ChatBox.AutoScroll = false;
            ChatBox.HorizontalScroll.Enabled = false;
            ChatBox.HorizontalScroll.Visible = false;
            ChatBox.HorizontalScroll.Maximum = 0;
            ChatBox.AutoScroll = true;


            byte[] message = Encoding.Unicode.GetBytes("ping;");
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);

            form7 = new Form7(client, ID);
        }

        private void Send_Broadcast_Click(object sender, EventArgs e)
        {
            form7.Visible = true;
        }

        private delegate void SafeCallDelegateChangeForm(RSAParameters privKey, RSAParameters pubKey);

        private void changeForm(RSAParameters privKey, RSAParameters pubKey)
        {
            if (this.InvokeRequired)
            {
                var d = new SafeCallDelegateChangeForm(changeForm);
                this.Invoke(d, new object[] { privKey, pubKey });
            }
            else
            {
                Form1 form = new Form1(privKey, pubKey, true, client, ID);
                form.Show();
                this.Close();
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            moving = true;
            changeForm(privKey, pubKey);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                byte[] message = Encoding.Unicode.GetBytes("adminSearch;" + UserIDSearch.Text + ";");
                Array.Resize(ref message, 8192);
                stream.BeginWrite(message, 0, 8192, null, null);
            }
        }
    }
}
