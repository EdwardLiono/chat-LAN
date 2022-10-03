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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }
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
        bool moving=false;
        private void Button2_Click(object sender, EventArgs e)
        {
            moving = true;
            byte[] message = Encoding.Unicode.GetBytes("ping;");
            Array.Resize(ref message, 8192);
            stream.BeginWrite(message, 0, 8192, null, null);
            Form2 form = new Form2(client);
            form.Show();
            this.Close();
        }
        private delegate void SafeCallDelegate(RSAParameters privKey, RSAParameters pubKey, bool isAdmin);

        private void changeForm(RSAParameters privKey, RSAParameters pubKey, bool isAdmin)
        {
            if (this.InvokeRequired)
            {
                var d = new SafeCallDelegate(changeForm);
                this.Invoke(d, new object[] { privKey, pubKey, isAdmin });
            }
            else
            {
                Form1 form = new Form1(privKey, pubKey, isAdmin, client, ID.Text);
                form.Show();
                this.Close();
            }
        }
        Int32 port = 13000;
        TcpClient client;
        NetworkStream stream;
        private byte[] receiveBuffer;
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (moving == false)
                {
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    string dataString = Encoding.Unicode.GetString(_data);

                    string[] dataSplit = dataString.Split(';');
                    if (dataSplit[0] == "success")
                    {
                        string privKeyString = DecryptAES(dataSplit[1].Trim(), ComputeSha256Hash(Password.Text));
                        RSAParameters privKey = StringToRSAParameter(privKeyString);
                        RSAParameters pubKey = StringToRSAParameter(dataSplit[2]);
                        byte[] message = Encoding.Unicode.GetBytes("ping;");
                        Array.Resize(ref message, 8192);
                        stream.BeginWrite(message, 0, 8192, null, null);

                        moving = true;
                        changeForm(privKey, pubKey,bool.Parse(dataSplit[3]));
                    }
                    else if (dataSplit[0] == "failed")
                    {
                        MessageBox.Show(dataSplit[1]);
                    }
                    else
                    {
                        MessageBox.Show(dataString);
                    }

                    if (moving == false)
                    {
                        stream.BeginRead(receiveBuffer, 0, 8192, ReceiveCallback, null);
                    }
                }
            }
            catch
            {
                if (moving == false)
                {
                    Disconnect();
                }
            }

        }


        private void Form3_Load(object sender, EventArgs e)
        {
            receiveBuffer = new byte[8192];

            client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), port);
            System.Threading.Thread.Sleep(100);
            stream = client.GetStream();
            if (moving == false)
            {
                stream.BeginRead(receiveBuffer, 0, 8192, ReceiveCallback, null);
            }
        }
        private void Disconnect()
        {
            if (client != null)
            {
                byte[] message = Encoding.Unicode.GetBytes("dc;");
                Array.Resize(ref message, 8192);
                stream.BeginWrite(message, 0, 8192, null, null);
                client.Close();
                client = null;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if(ID.Text!="" && Password.Text != "")
            {
                string hash1 = ComputeSha256Hash(Password.Text);
                string hash2 = ComputeSha256Hash(hash1);
                byte[] message = Encoding.Unicode.GetBytes("login;"+ID.Text+";"+hash2+";");
                Array.Resize(ref message, 8192);
                stream.BeginWrite(message, 0, 8192, null, null);
            }
            else
            {
                MessageBox.Show("Please fill everything");
            }
        }
    }
}
