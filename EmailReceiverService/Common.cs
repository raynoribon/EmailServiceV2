using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EmailReceiverService
{
    public static class Common
    {
        private static string key = "0x6ED5833CF35286EB";

        public static string GetConfig(string name)
        {
            System.Data.DataSet ds = new System.Data.DataSet();
            string filename = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\settings.xml";
            ds.ReadXml(filename, System.Data.XmlReadMode.ReadSchema);
            return ds.Tables["System"].Rows[0][name].ToString();
        }


        public static bool CreateFileFromBinary(string Filename, byte[] Data)
        {
            FileStream fs;
            BinaryWriter bw;
            int bufferSize;
            fs = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write);
            bw = new BinaryWriter(fs);

            bufferSize = Data.Count() - 1;
            bw.Write(Data, 0, bufferSize);
            bw.Flush();

            bw.Close();
            fs.Close();

            return true;
        }


        public static void Log(string msg)
        {
            string errorLogFilename = @"\LOG-" + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
            string full_path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory_path = System.IO.Path.GetDirectoryName(full_path) + @"\LOGS-Sender";
            string path = directory_path + errorLogFilename;

            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : Log file created");
                }
            }

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + " : " + msg);
            }
        }

        public static void ReceiverLog(string msg)
        {
            string errorLogFilename = @"\LOG-" + DateTime.Now.ToString("dd-MM-yyyy") + ".log";
            string full_path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory_path = System.IO.Path.GetDirectoryName(full_path) + @"\LOGS-Receiver";
            string path = directory_path + errorLogFilename;

            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : Log file created");
                }
            }

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + " : " + msg);
            }
        }



        public static string Left(string s, int left)
        {
            if (s == null) s = "";
            if (s.Length < left)
                return s.Substring(0, s.Length);
            else
                return s.Substring(0, left);
        }

        /// <param name="txt">String to be encrypted</param>
        public static string Encrypt(string txt)
        {
            if (txt == null)
            {
                return null;
            }

            if (key == null)
            {
                key = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(txt);
            var txtBytes = Encoding.UTF8.GetBytes(key);

            // Hash the password with SHA256
            txtBytes = SHA256.Create().ComputeHash(txtBytes);

            var bytesEncrypted = Encrypted(bytesToBeEncrypted, txtBytes);

            return Convert.ToBase64String(bytesEncrypted);
        }

        /// <param name="txt">String to be decrypted</param>
        /// <exception cref="FormatException"></exception>
        public static string Decrypt(string txt)
        {
            if (txt == null)
            {
                return null;
            }

            if (key == null)
            {
                key = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(txt);
            var passwordBytes = Encoding.UTF8.GetBytes(key);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = Decrypted(bytesToBeDecrypted, passwordBytes);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        private static byte[] Encrypted(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] Decrypted(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }


    }

    public struct _attachment
    {
        public string Filename;
        public string Filetype;
        public string GUID;
        public byte[] Data;
    }

    public struct _email
    {
        public string ID;
        public string From;
        public string To;
        public string Cc;
        public string Bcc;
        public string Subject;
        public string Body;
        public string Date;
        public string Uid;

        public List<_attachment> Attachments;
    }
}
