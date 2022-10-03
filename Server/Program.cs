using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using MySql.Data;
using System.Threading;



namespace Server2
{
    class Program
    {
        static Random rand = new Random();
        //private static MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;database=ChatDatabase;uid=root;pwd=Edward2894;");
        private static string connString = "server=127.0.0.1;database=ChatDatabase;uid={user};pwd={pwd};";
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
            for (int i = 0; i < length/2; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            if (builder.Length < length)
            {
                builder.Append(bytes[bytes.Length-1].ToString("x2")[0]);
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
        class Client
        {
            public static int dataBufferSize = 8192;

            public string id;
            public TCP tcp;
            public Client(string _id)
            {
                id = _id;
                tcp = new TCP(_id);
            }
            public class TCP
            {
                public TcpClient socket;

                private string id;
                private NetworkStream stream;
                private byte[] receiveBuffer;

                string msgKey = "";
                string file = "";

                public TCP(string _id)
                {
                    id = _id;
                }
                public void ChangeID(string _id)
                {
                    id = _id;
                }
                public void Connect(TcpClient _socket)
                {
                    socket = _socket;
                    socket.ReceiveBufferSize = dataBufferSize;
                    socket.SendBufferSize = dataBufferSize;

                    stream = socket.GetStream();

                    receiveBuffer = new byte[dataBufferSize];

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                private void ReceiveCallback(IAsyncResult _result)
                {
                    try
                    {
                        int _byteLength = stream.EndRead(_result);

                        byte[] _data = new byte[_byteLength];
                        Array.Copy(receiveBuffer, _data, _byteLength);

                        string dataString = Encoding.Unicode.GetString(_data);


                        string[] dataSplit = dataString.Split(';');
                        if (dataSplit[0] == "msg")
                        {
                            string key = GenerateRandomString(24);
                            msgKey = key;

                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT Public FROM ChatDatabase.keys WHERE UserId = @UserId;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                            RSAParameters pubkey = StringToRSAParameter((string)comm.ExecuteScalar());
                            conn.Close();
                            string encryptedkey = EncryptRSA(key, pubkey);
                            if (dataSplit[2] == "false")
                            {
                                Server.Send(id, "msg2;" + encryptedkey + ";");
                            }
                            else
                            {
                                Server.Send(id, "filemsg2;" + encryptedkey + ";");

                            }
                            
                        }
                        else if(dataSplit[0] == "msgSend")
                        {
                            //TargetID;IsGroup;isFile;Time;Message;
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT COUNT(*) FROM ChatDatabase.messages;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            int count = (int)(long)comm.ExecuteScalar();

                            query = "INSERT INTO ChatDatabase.messages(MessageID, SenderID, TargetID, IsGroup, Message, isFile, isImage, Time) VALUES (@Count, @SenderID, @TargetID, @IsGroup, @Message, @isFile, false, @Time);";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Count", count));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@SenderID", id));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", dataSplit[1]));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@IsGroup", bool.Parse(dataSplit[2])));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Message", dataSplit[5]));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@isFile", bool.Parse(dataSplit[3])));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Time", DateTime.Parse(dataSplit[4])));
                            comm.ExecuteNonQuery();

                            MySql.Data.MySqlClient.MySqlDataReader reader;
                            List<string> targets = new List<string>();
                            if (bool.Parse(dataSplit[2]) == false)
                            {
                                targets.Add(id);
                                targets.Add(dataSplit[1]);
                            }
                            else
                            {
                                query = "SELECT UserID FROM ChatDatabase.groupmember WHERE GroupID=@GroupID;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@GroupID", dataSplit[1]));
                                reader = comm.ExecuteReader();
                                while (reader.Read())
                                {
                                    targets.Add(reader.GetString(0));
                                }
                                reader.Close();
                            }
                            List<string> nosend = new List<string>();
                            query = "SELECT UserID FROM ChatDatabase.accounts WHERE IsAdmin=true;";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                nosend.Add(reader.GetString(0));
                            }
                            reader.Close();

                            query = "SELECT Name FROM ChatDatabase.accounts WHERE UserId=@UserId;";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", id));
                            string name = (string)comm.ExecuteScalar();
                            query = "SELECT Divison FROM ChatDatabase.accounts WHERE UserId=@UserId;";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", id));
                            //string Division = (string)comm.ExecuteScalar();


                            for (int i = 0; i < targets.Count; i++)
                            {
                                query = "SELECT Public FROM ChatDatabase.keys WHERE UserId = @UserId;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", targets[i]));
                                RSAParameters pubkey = StringToRSAParameter((string)comm.ExecuteScalar());
                                string enckey = EncryptRSA(msgKey, pubkey);
                                comm = new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(*) FROM ChatDatabase.messagekey", conn);
                                int count2 = (int)(long)comm.ExecuteScalar();
                                query = "INSERT INTO ChatDatabase.messagekey(Id, MessageID, TargetID, `Key`) VALUES (@Count, @MessageID, @TargetID, @Key);";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Count", count2));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", count.ToString()));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", targets[i]));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Key", enckey));
                                comm.ExecuteNonQuery();
                                if (Server.clients.ContainsKey(targets[i]))
                                {
                                    Server.Send(targets[i], "msg;" + enckey + ";" + dataSplit[1] + ";" + id + ";" + name + ";" + dataSplit[5] + ";");
                                }
                            }
                            for (int i = 0; i < nosend.Count; i++)
                            {
                                query = "SELECT Public FROM ChatDatabase.keys WHERE UserId = @UserId;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", nosend[i]));
                                RSAParameters pubkey = StringToRSAParameter((string)comm.ExecuteScalar());
                                string enckey = EncryptRSA(msgKey, pubkey);
                                comm = new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(*) FROM ChatDatabase.messagekey", conn);
                                int count2 = (int)(long)comm.ExecuteScalar();
                                query = "INSERT INTO ChatDatabase.messagekey(Id, MessageID, TargetID, `Key`) VALUES (@Count, @MessageID, @TargetID, @Key);";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Count", count2));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", count.ToString()));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", nosend[i]));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Key", enckey));
                                comm.ExecuteNonQuery();
                            }
                            conn.Close();
                            msgKey = "";    
                        }
                        else if (dataSplit[0] == "fileSend")
                        {
                            //fileSend;true;TargetID;isGroup;isFile;isImage;timeSent;filename;
                            if (dataSplit[1] == "false")
                            {
                                file += dataSplit[2];
                            }
                            else
                            {
                                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                                conn.Open();
                                string query = "SELECT COUNT(*) FROM ChatDatabase.Messages;";
                                MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                int count = (int)(long)comm.ExecuteScalar();

                                Directory.CreateDirectory("files");
                                using (StreamWriter writer = new StreamWriter("files\\"+count.ToString()+"."+dataSplit[7]+".txt"))
                                {
                                    writer.WriteLine(file);
                                }

                                query = "INSERT INTO ChatDatabase.Messages(MessageID, SenderID, TargetID, IsGroup, Message, isFile, isImage, Time) VALUES (@Count, @SenderID, @TargetID, @IsGroup, @Message, @isFile, @isImage, @Time);";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Count", count));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@SenderID", id));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", dataSplit[2]));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@IsGroup", bool.Parse(dataSplit[3])));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Message", count.ToString() + "." + dataSplit[7]));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@isFile", bool.Parse(dataSplit[4])));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@isImage", bool.Parse(dataSplit[5])));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Time", DateTime.Parse(dataSplit[6])));
                                comm.ExecuteNonQuery();

                                MySql.Data.MySqlClient.MySqlDataReader reader;
                                List<string> targets = new List<string>(); 
                                if (dataSplit[3] == "false")
                                {
                                    targets.Add(id);
                                    targets.Add(dataSplit[2]);
                                }
                                else
                                {
                                    query = "SELECT UserID FROM ChatDatabase.Groupmember WHERE GroupID=@GroupID;";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@GroupID", dataSplit[1]));
                                    reader = comm.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        targets.Add(reader.GetString(0));
                                    }
                                    reader.Close();
                                }
                                List<string> nosend = new List<string>();
                                query = "SELECT UserID FROM ChatDatabase.Accounts WHERE IsAdmin=true;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                reader = comm.ExecuteReader();
                                while (reader.Read())
                                {
                                    nosend.Add(reader.GetString(0));
                                }
                                reader.Close();

                                query = "SELECT Name FROM ChatDatabase.accounts WHERE UserId=@UserId;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", id));
                                string name = (string)comm.ExecuteScalar();
                                query = "SELECT Name FROM ChatDatabase.accounts WHERE UserId=@UserId;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", id));
                                string Division = (string)comm.ExecuteScalar();


                                for (int i = 0; i < targets.Count; i++)
                                {
                                    query = "SELECT Public FROM ChatDatabase.keys WHERE UserId = @UserId;";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", targets[i]));
                                    RSAParameters pubkey = StringToRSAParameter((string)comm.ExecuteScalar());
                                    string enckey = EncryptRSA(msgKey, pubkey);
                                    comm = new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(*) FROM ChatDatabase.messagekey", conn);
                                    int count2 = (int)(long)comm.ExecuteScalar();
                                    query = "INSERT INTO ChatDatabase.messagekey(Id, MessageID, TargetID, `Key`) VALUES (@Count, @MessageID, @TargetID, @Key);";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Count", count2));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", count.ToString()));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", targets[i]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Key", enckey));
                                    comm.ExecuteNonQuery();
                                    if (Server.clients.ContainsKey(targets[i]))
                                    {
                                        if (dataSplit[5] == "false")
                                        {
                                            Server.Send(targets[i], "filemsg;" + enckey + ";" + dataSplit[2] + ";" + id + ";" + name + ";" + count.ToString() + "." + dataSplit[7] + ";");
                                        }
                                        else
                                        {
                                            int numBatch = (file.Length + 3999) / 4000;
                                            for (int j = 0; j < numBatch; j++)
                                            {
                                                Server.Send(targets[i], "imgmsg;false;"+ count.ToString() + "." + dataSplit[7] + ";" + file.Substring(j * 4000, Math.Min(4000, file.Length - j * 4000))+";");
                                            }

                                            Server.Send(targets[i], "imgmsg;true;" + enckey + ";" + dataSplit[2] + ";" + id + ";" + name + ";" + count.ToString() + "." + dataSplit[7] + ";");
                                        }
                                    }
                                    
                                }
                                for (int i = 0; i < nosend.Count; i++)
                                {
                                    query = "SELECT Public FROM ChatDatabase.keys WHERE UserId = @UserId;";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", nosend[i]));
                                    RSAParameters pubkey = StringToRSAParameter((string)comm.ExecuteScalar());
                                    string enckey = EncryptRSA(msgKey, pubkey);
                                    comm = new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(*) FROM ChatDatabase.messagekey", conn);
                                    int count2 = (int)(long)comm.ExecuteScalar();
                                    query = "INSERT INTO ChatDatabase.messagekey(Id, MessageID, TargetID, `Key`) VALUES (@Count, @MessageID, @TargetID, @Key);";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Count", count2));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", count.ToString()));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", nosend[i]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Key", enckey));
                                    comm.ExecuteNonQuery();
                                }
                                file = "";
                                msgKey = "";
                                conn.Close();
                            }
                        }
                        else if (dataSplit[0] == "downloadfile")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT `Key` FROM ChatDatabase.MessageKey WHERE MessageID = @MessageID AND TargetID = @TargetID;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", dataSplit[1].Split('.')[0]));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", id));
                            string key = comm.ExecuteScalar() as string;
                            string fileup;
                            using (StreamReader reader = new StreamReader("files\\"+dataSplit[1]+".txt"))
                            {
                                fileup=reader.ReadToEnd();
                            }
                            int numBatch = (fileup.Length + 3999) / 4000;
                            for (int j = 0; j < numBatch; j++)
                            {
                                Server.Send(id, "downloadfile;false;" + dataSplit[1] + ";" + fileup.Substring(j * 4000, Math.Min(4000, fileup.Length - j * 4000)) + ";");
                            }

                            Server.Send(id, "downloadfile;true;" + dataSplit[1] + ";" + key + ";");
                            conn.Close();
                        }
                        else if (dataSplit[0] == "dc")
                        {
                            Server.clients[id].Disconnect();
                            Console.WriteLine("Disconnected; "+string.Join(",", Server.clients.Keys));
                        }
                        else if (dataSplit[0] == "changeid")
                        {
                            string oldID = id;
                            Server.clients[id].ChangeID(dataSplit[1]);
                            Server.clients.Add(dataSplit[1], Server.clients[oldID]);
                            Server.clients.Remove(oldID);
                        }
                        else if (dataSplit[0] == "newacc")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            bool adminAllow=false;
                            if (bool.Parse(dataSplit[4]))
                            {
                                string query = "SELECT COUNT(1) FROM ChatDatabase.accounts WHERE IsAdmin = true;";
                                MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                if ((int)(long)comm.ExecuteScalar() == 0)
                                {
                                    adminAllow = true;
                                }
                                if (adminAllow == false)
                                {
                                    query = "select Salt from ChatDatabase.accounts where UserId = @UserId;";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[6]));
                                    string saltAdmin = (string)comm.ExecuteScalar();
                                    string hashpassAdmin = ComputeSha256Hash(dataSplit[7] + saltAdmin);
                                    query = "SELECT COUNT(1) FROM ChatDatabase.accounts WHERE UserId = @UserId AND Password = @Password AND isAdmin = true;";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[6]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Password", hashpassAdmin));
                                    if ((int)(long)comm.ExecuteScalar() == 1)
                                    {
                                        adminAllow = true;
                                    }
                                }
                            }
                            else
                            {
                                adminAllow = true;
                            }
                            if (adminAllow || bool.Parse(dataSplit[4])==false)
                            {
                                string query = "SELECT COUNT(1) FROM ChatDatabase.accounts WHERE UserId = @UserId;";
                                MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                if ((int)(long)comm.ExecuteScalar() == 0)
                                {
                                    string hash1 = dataSplit[5];
                                    string hash2 = ComputeSha256Hash(hash1);
                                    string salt = GenerateRandomString(64);
                                    string hash3 = ComputeSha256Hash(hash2 + salt);
                                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                                    string publicKey = RSAParameterToString(rsa.ExportParameters(false));
                                    string privateKey = EncryptAES(RSAParameterToString(rsa.ExportParameters(true)), hash1);
                                    query = "INSERT INTO ChatDatabase.Accounts(UserId, Name, Division, Password, Salt, IsAdmin) VALUES (@UserId, @Name, @Division, @Password," +
                                        " @Salt, @IsAdmin)";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Name", dataSplit[3]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Division", dataSplit[2]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Password", hash3));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Salt", salt));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@IsAdmin", bool.Parse(dataSplit[4])));
                                    comm.ExecuteNonQuery();
                                    query = "INSERT INTO ChatDatabase.Keys(UserId, Public, Private) VALUES (@UserId, @Public, @Private)";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Public", publicKey));
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Private", privateKey));
                                    comm.ExecuteNonQuery();
                                    string oldID = id;
                                    Server.clients[id].ChangeID(dataSplit[1]);
                                    Server.clients.Add(dataSplit[1], Server.clients[oldID]);
                                    Server.clients.Remove(oldID);
                                    Server.Send(id, "success;" + privateKey + ";"+publicKey+";");
                                }
                                else
                                {
                                    Server.Send(id, "failed;The UserID already exists;");
                                }
                                conn.Close();
                            }
                            else
                            {
                                Server.Send(id, "failed;Admin password doesnt match;");
                            }
                        }
                        else if (dataSplit[0] == "login")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT COUNT(1) FROM ChatDatabase.accounts WHERE UserId = @UserId";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                            if ((int)(long)comm.ExecuteScalar() == 1)
                            {
                                query = "select Salt from ChatDatabase.accounts where UserId = @UserId;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                string salt = (string)comm.ExecuteScalar();
                                string hashpass = ComputeSha256Hash(dataSplit[2] + salt);
                                query = "SELECT COUNT(1) FROM ChatDatabase.accounts WHERE UserId = @UserId AND Password = @Password";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Password", hashpass));
                                if((int)(long)comm.ExecuteScalar() == 1)
                                {
                                    query = "select Private from ChatDatabase.keys where UserId=@UserId";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                    string privateKey= (string)comm.ExecuteScalar();
                                    query = "select Public from ChatDatabase.keys where UserId=@UserId";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                    string publicKey = (string)comm.ExecuteScalar();
                                    query = "select isAdmin from ChatDatabase.accounts where UserId=@UserId";
                                    comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                    comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                                    object test = comm.ExecuteScalar();
                                    string isAdmin = Convert.ToBoolean(comm.ExecuteScalar()).ToString();
                                    string oldID = id;
                                    Server.clients[id].ChangeID(dataSplit[1]);
                                    Server.clients.Add(dataSplit[1], Server.clients[oldID]);
                                    Server.clients.Remove(oldID);
                                    Server.Send(id, "success;" + privateKey + ";" + publicKey + ";" + isAdmin+";");
                                }
                                else
                                {
                                    Server.Send(id, "failed;Password doesn't match;");
                                }
                            }
                            else
                            {
                                Server.Send(id, "failed;User not found;");
                            }
                            conn.Close();
                        }
                        else if (dataSplit[0] == "ping")
                        {
                            Server.Send(id, "ping;");
                        }
                        else if (dataSplit[0] == "find")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID LIKE @UserID;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", "%"+dataSplit[2]+"%"));
                            MySql.Data.MySqlClient.MySqlDataReader reader = comm.ExecuteReader();
                            string result = "find;"+dataSplit[1]+";";
                            while (reader.Read()&&result.Length<4000)
                            {
                                result += reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";";
                            }
                            reader.Close();
                            Server.Send(id, "ping;");
                            Server.Send(id, result);
                            conn.Close();
                        }
                        else if (dataSplit[0] == "add")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID = @UserID;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", dataSplit[1]));
                            MySql.Data.MySqlClient.MySqlDataReader reader = comm.ExecuteReader();
                            reader.Read();
                            string result = "add;"+ reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";";
                            Server.Send(id, result);
                            reader.Close();
                            if (Server.clients.ContainsKey(dataSplit[1]))
                            {
                                query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID = @UserID;";
                                comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserId", id));
                                reader = comm.ExecuteReader();
                                reader.Read();
                                result = "add;" + reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";";
                                Server.Send(dataSplit[1], "ping;");
                                Server.Send(dataSplit[1], result);
                                reader.Close();
                            }
                            conn.Close();
                        }
                        else if (dataSplit[0] == "makeGroup")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string name = dataSplit[1];
                            string query = "SELECT COUNT(*) FROM ChatDatabase.group;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            int count = (int)(long)comm.ExecuteScalar();
                            query = "INSERT INTO ChatDatabase.group(GroupID, Name) VALUES (@ID, @Name)";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", count));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Name", name));
                            comm.ExecuteNonQuery();
                            query = "INSERT INTO ChatDatabase.groupmember(ID, UserID, GroupID) SELECT COUNT(*), @UserID, @GroupID FROM ChatDatabase.groupmember";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            for (int i = 2; i < dataSplit.Length-1; i++)
                            {
                                comm.Parameters.Clear();
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@UserID", dataSplit[i]));
                                comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@GroupID", count));
                                comm.ExecuteNonQuery();
                                Server.Send(dataSplit[i], "ping;");
                                Server.Send(dataSplit[i], "newGroup;" + count + ";" + name + ";");
                            }
                            conn.Close();
                        }
                        else if(dataSplit[0] == "load")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            DateTime loadTime = DateTime.Parse(dataSplit[1]);
                            string result = "";

                            Dictionary<string,bool> check=new Dictionary<string,bool>();

                            string query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID = ANY (SELECT DISTINCT SenderID FROM ChatDatabase.messages WHERE TargetID = @ID AND IsGroup = False AND Time>@Time)";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", id));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Time", loadTime));
                            MySql.Data.MySqlClient.MySqlDataReader reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                result+="group;false;"+ reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";\n";
                                check.Add(reader.GetString(0), true);
                            }
                            reader.Close();
                            query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID = ANY (SELECT DISTINCT TargetID FROM ChatDatabase.messages WHERE SenderID = @ID AND IsGroup = False AND Time>@Time)";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", id));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Time", loadTime));
                            reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                if (!check.ContainsKey(reader.GetString(0)))
                                {
                                    result += "group;false;" + reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";\n"; 
                                }
                            }
                            reader.Close();
                            query = "SELECT GroupID, Name FROM ChatDatabase.Group WHERE GroupID = ANY (SELECT GroupID FROM ChatDatabase.GroupMember WHERE UserID = @ID)";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", id));
                            reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                result += "group;true;" + reader.GetString(0) + ";" + reader.GetString(1) + ";\n";
                            }
                            reader.Close();
                            query = "SELECT MessageID, SenderID, TargetID, IsGroup, Message, `Time`, isFile, isImage FROM Chatdatabase.messages WHERE `Time`>@Time AND (TargetID = @ID OR SenderID = @ID OR TargetID = ANY (SELECT GroupID FROM ChatDatabase.GroupMember WHERE UserID = @ID)) ORDER BY TargetID, `Time` ASC";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", id));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Time", loadTime));
                            reader = comm.ExecuteReader();
                            
                            while (reader.Read())
                            {
                                MySql.Data.MySqlClient.MySqlConnection conn2 = new MySql.Data.MySqlClient.MySqlConnection(connString);
                                conn2.Open();
                                query = "SELECT `Key` FROM Chatdatabase.messagekey WHERE MessageID=@MessageID AND TargetID = @TargetID";
                                MySql.Data.MySqlClient.MySqlCommand comm2 = new MySql.Data.MySqlClient.MySqlCommand(query, conn2);
                                comm2.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", reader.GetString(0)));
                                comm2.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", id));
                                string key=comm2.ExecuteScalar().ToString();
                                query = "SELECT Name FROM Chatdatabase.accounts WHERE UserId=@ID";
                                comm2 = new MySql.Data.MySqlClient.MySqlCommand(query, conn2);
                                comm2.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", reader.GetString(1)));
                                string senderName=comm2.ExecuteScalar().ToString();
                                result += "message;" + reader.GetString(1) + ";" + reader.GetString(2) + ";" + reader.GetBoolean(3).ToString() + ";" + reader.GetString(4) + ";" + reader.GetDateTime(5).ToString() + ";" + reader.GetBoolean(6).ToString() + ";" + reader.GetBoolean(7).ToString()+";"+key+";"+senderName+";";
                                if (reader.GetBoolean(7))
                                {
                                    string file = new StreamReader("files\\"+reader.GetString(4)+".txt").ReadToEnd();
                                    result += file + ";";
                                }
                                result += "\n";
                                conn2.Close();
                            }
                            reader.Close();
                            int numBatch = (result.Length + 3999) / 4000;

                            Server.Send(id, "ping;");
                            for (int j = 0; j < numBatch; j++)
                            {
                                Console.WriteLine("load;false;" + result.Substring(j * 4000, Math.Min(4000, result.Length - j * 4000)) + ";");
                                Server.Send(id, "load;false;"+ result.Substring(j * 4000, Math.Min(4000, result.Length - j * 4000))+";");
                            }
                            Server.Send(id, "load;true;");
                            conn.Close();
                        }
                        else if (dataSplit[0] == "adminSearch")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string result = "";

                            Dictionary<string, bool> check = new Dictionary<string, bool>();

                            string query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID = ANY (SELECT DISTINCT SenderID FROM ChatDatabase.messages WHERE TargetID = @ID AND IsGroup = False)";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", dataSplit[1]));
                            MySql.Data.MySqlClient.MySqlDataReader reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                result += "group;false;" + reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";\n";
                            }
                            reader.Close();
                            query = "SELECT UserID,Name, Division FROM ChatDatabase.accounts WHERE UserID = ANY (SELECT DISTINCT TargetID FROM ChatDatabase.messages WHERE SenderID = @ID AND IsGroup = False)";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", dataSplit[1]));
                            reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                if (!check.ContainsKey(reader.GetString(0)))
                                {
                                    result += "group;false;" + reader.GetString(0) + ";" + reader.GetString(1) + ";" + reader.GetString(2) + ";\n";
                                }
                            }
                            reader.Close();
                            query = "SELECT GroupID, Name FROM ChatDatabase.Group WHERE GroupID = ANY (SELECT GroupID FROM ChatDatabase.GroupMember WHERE UserID = @ID)";
                            comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", dataSplit[1]));
                            reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                result += "group;true;" + reader.GetString(0) + ";" + reader.GetString(1) + ";\n";
                            }
                            reader.Close();
                            int numBatch = (result.Length + 3999) / 4000;
                            Server.Send(id, "ping;");
                            for (int j = 0; j < numBatch; j++)
                            {
                                Server.Send(id, "adminSearch;false;" + result.Substring(j * 4000, Math.Min(4000, result.Length - j * 4000)) + ";");
                                
                            }
                            Server.Send(id, "adminSearch;true;");
                            conn.Close();
                        }
                        else if (dataSplit[0] == "adminMessageLoad")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string result = "";

                            string query = "SELECT MessageID, SenderID, TargetID, IsGroup, Message, `Time`, isFile, isImage FROM Chatdatabase.messages WHERE `Time`>@Time AND (TargetID = @ID OR SenderID = @ID) ORDER BY TargetID, `Time` ASC";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", dataSplit[1]));
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@Time", DateTime.Parse(dataSplit[2])));
                            MySql.Data.MySqlClient.MySqlDataReader reader = comm.ExecuteReader();
                            while (reader.Read())
                            {
                                MySql.Data.MySqlClient.MySqlConnection conn2 = new MySql.Data.MySqlClient.MySqlConnection(connString);
                                conn2.Open();
                                query = "SELECT `Key` FROM Chatdatabase.messagekey WHERE MessageID=@MessageID AND TargetID = @TargetID";
                                MySql.Data.MySqlClient.MySqlCommand comm2 = new MySql.Data.MySqlClient.MySqlCommand(query, conn2);
                                comm2.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@MessageID", reader.GetString(0)));
                                comm2.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@TargetID", id));
                                object keyObj = comm2.ExecuteScalar();
                                if (keyObj != null)
                                {
                                    string key = keyObj.ToString();
                                    query = "SELECT Name FROM Chatdatabase.accounts WHERE UserId=@ID";
                                    comm2 = new MySql.Data.MySqlClient.MySqlCommand(query, conn2);
                                    comm2.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", reader.GetString(1)));
                                    string senderName = comm2.ExecuteScalar().ToString();
                                    result += "message;" + reader.GetString(1) + ";" + reader.GetString(2) + ";" + reader.GetBoolean(3).ToString() + ";" + reader.GetString(4) + ";" + reader.GetDateTime(5).ToString() + ";" + reader.GetBoolean(6).ToString() + ";" + reader.GetBoolean(7).ToString() + ";" + key + ";" + senderName + ";";
                                    if (reader.GetBoolean(7))
                                    {
                                        string file = new StreamReader("files\\" + reader.GetString(4) + ".txt").ReadToEnd();
                                        result += file + ";";
                                    }
                                    result += "\n";
                                }
                                conn2.Close();
                            }
                            reader.Close();
                            int numBatch = (result.Length + 3999) / 4000;
                            
                            for (int j = 0; j < numBatch; j++)
                            {
                                Server.Send(id, "adminMessageLoad;false;" + result.Substring(j * 4000, Math.Min(4000, result.Length - j * 4000)) + ";");
                            }
                            Server.Send(id, "adminMessageLoad;true;"+dataSplit[1]+";");
                            conn.Close();
                        }
                        else if(dataSplit[0] == "adminbroadcast")
                        {
                            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
                            conn.Open();
                            string query = "SELECT Name FROM Chatdatabase.accounts WHERE UserID=@ID;";
                            MySql.Data.MySqlClient.MySqlCommand comm = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                            comm.Parameters.Add(new MySql.Data.MySqlClient.MySqlParameter("@ID", dataSplit[1]));
                            string name = comm.ExecuteScalar().ToString();
                            conn.Close();
                            foreach(string keys in Server.clients.Keys)
                            {
                                Server.Send(keys, "adminBroadcast;" + name + ";" + dataSplit[1] + ";" + dataSplit[2] + ";");
                            }
                        }
                        if (stream != null)
                        {
                            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                        }
                    }
                    
                    catch (Exception _ex)
                    {
                        if (Server.clients.ContainsKey(id))
                        {
                            Console.WriteLine($"Error receiving TCP data: {_ex}");
                            Server.clients[id].Disconnect();
                        }
                    }
                }


                public void SendData(byte[] _packet)
                {
                    try
                    {
                        if (socket != null)
                        {
                            stream.BeginWrite(_packet, 0, _packet.Length, null, null);
                        }
                    }
                    catch (Exception _ex)
                    {
                        Console.WriteLine($"Error sending data to client {id} via TCP: {_ex}");
                    }
                }
                public void Disconnect()
                {

                    socket.Close();
                    stream = null;
                    receiveBuffer = null;
                    socket = null;
                }
            }
            private void ChangeID(string _id)
            {
                id = _id;
                tcp.ChangeID(_id);
            }
            private void Disconnect()
            {
                Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
                tcp.Disconnect();
                Server.clients.Remove(id);
            }

        }
        class Server
        {
            public static int Port;
            public static Dictionary<string, Client> clients = new Dictionary<string, Client>();
            private static TcpListener tcpListener;

            public static void Start(int _port)
            {
                Port = _port;
                Console.WriteLine("Starting server...");

                tcpListener = new TcpListener(IPAddress.Any, Port);

                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

                Console.WriteLine($"Server started on port {Port}.");
            }
            private static void TCPConnectCallback(IAsyncResult _result)
            {
                TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
                Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

                int i = 0;
                while (i < 1000)
                {
                    if (!clients.ContainsKey(i.ToString()))
                    {
                        clients.Add(i.ToString(), new Client(i.ToString()));
                        clients[i.ToString()].tcp.Connect(_client);
                        return;
                    }
                    i++;
                }
            }
            public static void Send(string _id, string _message)
            {
                if (clients.ContainsKey(_id))
                {
                    byte[] sendMessage = Encoding.Unicode.GetBytes(_message);
                    Array.Resize(ref sendMessage, 8192);
                    clients[_id].tcp.SendData(sendMessage);
                }
            }
        }
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Server.Start(13000);
            Console.ReadKey();
        }
    }
}
