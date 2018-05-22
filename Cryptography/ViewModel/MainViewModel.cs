using Cryptography.Common;
using Cryptography.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;



namespace Cryptography.ViewModel
{



    public class MainViewModel : ViewModelBase
    {
        //TODO przetestować i ukryty folder
        #region Properties
        public string Login { get; set; }
        public string Password { get; set; }
        public string DecryptPassword { get; set; }
        public int FeedbackSize { get; set; }
        public int[] FeedbackSizes { get; set; }
        private string destinationFolder;
        public string DestinationFolder
        {
            get { return destinationFolder; }
            set { destinationFolder = value; OnPropertyChanged("DestinationFolder"); }
        }
        public string OutputFilename { get; set; }
        private string sourceFile;
        public string SourceFile
        {
            get { return sourceFile; }
            set { sourceFile = value; OnPropertyChanged("SourceFile"); }
        }
        private long fileSize;
        public string FileSize
        {
            get { 
                if (fileSize > 1073741824)
                {
                    return fileSize / 1073741824 + " GB";
                }
                else if (fileSize > 1048576)
                {
                    return fileSize / 1048576 + " MB";
                }
                else if (fileSize > 1024)
                {
                    return fileSize / 1024 + " KB";
                }
                else
                {
                    return fileSize + " B";
                }
            }
            set { fileSize = long.Parse((value.Split(' '))[0]); OnPropertyChanged("FileSize"); }
        }
        private string applicationStatus;
        public string ApplicationStatus
        {
            get { return applicationStatus; }
            set { applicationStatus = value; OnPropertyChanged("ApplicationStatus"); }
        }
        private int cryptProgress;
        public int CryptProgress
        {
            get { return cryptProgress; }
            set { cryptProgress = value; OnPropertyChanged("CryptProgress"); }
        }

        private ApplicationModeEnum applicationMode;
        public ApplicationModeEnum ApplicationMode
        {
            get { return applicationMode; }
            set
            {
                if (value != 0)
                    applicationMode = value;
                OnPropertyChanged("ApplicationMode");
                OnPropertyChanged("ListBoxUsers");
            }
        }

        private CryptBlockModeEnum cryptBlockMode;
        public CryptBlockModeEnum CryptBlockMode
        {
            get { return cryptBlockMode; }
            set
            {
                if (value != 0)
                    cryptBlockMode = value;
                OnPropertyChanged("CryptBlockMode");
            }
        }

        public RelayCommand Start { get; set; }
        public bool StartCanExecute { get; set; }
        public RelayCommand AddUser { get; set; }
        public RelayCommand DeleteUser { get; set; }
        public RelayCommand BrowseSourceFile { get; set; }
        public RelayCommand BrowseDestinationFolder { get; set; }
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<User> ApprovedUsers
        {
            get
            {
                ObservableCollection<User> result = new ObservableCollection<User>();
                if (!string.IsNullOrWhiteSpace(SourceFile) && Users != null)
                {
                    Dictionary<string, string> approvedUsers = ReadHeader(SourceFile);

                    foreach (var au in approvedUsers)
                    {
                        foreach (User u in Users)
                        {
                            if (u.Login.Equals(au.Key)) result.Add(u);
                        }
                    }
                }
                return result;
            }
        }
        public ObservableCollection<User> ListBoxUsers
        {
            get {
                if (ApplicationMode == ApplicationModeEnum.Decrypt) return ApprovedUsers;
                else if (ApplicationMode == ApplicationModeEnum.Encrypt) return Users;
                else return null;
            }
            
        }

        public Aes aes;
        #endregion
        const string privateKeyFolder = "Prywatne/";
        const string publicKeyFolder = "Publiczne/";
        const string usersFilename = "users.xml";

        public MainViewModel()
        {
            Users = new ObservableCollection<User>();
            //ApprovedUsers = new ObservableCollection<User>();
            ImportUsersXML(usersFilename, Users);
            Users.CollectionChanged += (s, e) => { ExportUsersXML(usersFilename, Users); OnPropertyChanged("ListBoxUsers"); };
            ApprovedUsers.CollectionChanged += (s, e) => { OnPropertyChanged("ListBoxUsers"); };
            OnPropertyChanged("ListBoxUsers");

            Login = "Login";
            Password = "Haslo";
            ApplicationMode = ApplicationModeEnum.Encrypt;
            CryptBlockMode = CryptBlockModeEnum.OFB;
            SourceFile = @"C:\Users\Tomasz\Desktop\Contact ang.txt";
            DestinationFolder = @"C:\Users\Tomasz\Desktop";
            OutputFilename = "Przerobiony4";          
            FileSize = "0 B";
            ApplicationStatus = "";
            CryptProgress = 0;
            FeedbackSizes = new int[] { 8, 16, 32, 64 };
            FeedbackSize = FeedbackSizes[0];
            StartCanExecute = true;
            Start = new RelayCommand(StartAction); //, (o) => { return StartCanExecute && Users.Count>0; });
            AddUser = new RelayCommand(AddUserAction);
            DeleteUser = new RelayCommand(DeleteUserAction);
            BrowseSourceFile = new RelayCommand(BrowseSourceFileAction);
            BrowseDestinationFolder = new RelayCommand(BrowseDestinationFolderAction);
            aes = Aes.Create();
            aes.GenerateKey();
            

        }


        private async void StartAction(object obj)
        {

            StartCanExecute = false;
            OnPropertyChanged("StartCanExecute");
            CryptProgress = 0;
            string destinationFile;
            List<User> recivers = GetSelectedUsers();
            aes.Padding = PaddingMode.Zeros;
            
            byte[] k = aes.Key;
            switch (ApplicationMode)
            {
                case ApplicationModeEnum.Encrypt:

                    switch (CryptBlockMode)
                    {
                        case CryptBlockModeEnum.ECB:
                            aes.Mode = CipherMode.ECB;
                            break;
                        case CryptBlockModeEnum.CBC:
                            aes.Mode = CipherMode.CBC;
                            break;
                        case CryptBlockModeEnum.CFB:
                            aes.Mode = CipherMode.CFB;
                            aes.FeedbackSize = FeedbackSize;
                            break;
                        case CryptBlockModeEnum.OFB:
                            aes.Mode = CipherMode.OFB;
                            aes.FeedbackSize = FeedbackSize;
                            break;
                        default:
                            break;
                    }
                    aes.KeySize = 256;
                    aes.GenerateIV();
                    aes.Key = GenerateKey(32);
                    string extension = Path.GetExtension(SourceFile);                   
                    destinationFile = DestinationFolder + "\\" + OutputFilename + ".txt";
                    WriteHeader(destinationFile, aes, recivers, extension);

                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    await EncryptFileToFile(SourceFile, destinationFile, encryptor);
                    ApplicationStatus = "Szyfrowanie zakonczone";
                    break;
                case ApplicationModeEnum.Decrypt:
                    //aes.Padding = PaddingMode.Zeros;
                    string extensionD;
                    Dictionary<string,string> approvedUsers = ReadHeader(SourceFile, aes, out extensionD);
                    User decryptorUser = recivers[0];
                    byte[] sessionKey = NumberStringToByteArray(approvedUsers[decryptorUser.Login], ' ');
                    aes.Key = DecryptSesionKey(sessionKey, decryptorUser);
                    //TODO aes.key od usera
                    destinationFile = DestinationFolder + "\\" + OutputFilename + extensionD;
                    //aes.Key = k;
                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    await DecryptFileToFile(SourceFile, destinationFile, decryptor);
                    ApplicationStatus = "Deszyfrowanie zakonczone";
                    break;
                default:
                    break;
            }
            CryptProgress = 100;
            StartCanExecute = true;
            OnPropertyChanged("StartCanExecute");
        }

        private byte[] DecryptSesionKey(byte[] sessionKey, User decryptorUser)
        {
            string privateRSA = DecryptPrivateRsa(decryptorUser);
            if (!string.IsNullOrWhiteSpace(privateRSA))
            {
                RSA rsa = RSA.Create();
                try
                {
                    rsa.FromXmlString(privateRSA); //Zaszyfrowuję cały klucz razem z xml i on potem tego błędnego klucza nie moze odczytac bo nie moze odczytac xmla
                    

                } catch
                {
                    return GenerateKey(32);
                }
                return rsa.Decrypt(sessionKey, RSAEncryptionPadding.OaepSHA1);



            }
            return null;
        }

        private byte[] EncryptSesionKey(byte[] sessionKey, User decryptorUser)
        {
            string publicRSA = GetPublicKey(decryptorUser);
            if (!string.IsNullOrWhiteSpace(publicRSA))
            {
                RSA rsa = RSA.Create();
                rsa.FromXmlString(publicRSA);
                return rsa.Encrypt(sessionKey, RSAEncryptionPadding.OaepSHA1);
            }
            return null;
        }

        private string DecryptPrivateRsa(User decryptorUser)
        {
            if (!string.IsNullOrWhiteSpace(DecryptPassword))
            {
                byte[] password = GetHash(DecryptPassword);
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = password;
                return DecryptFileToString(/*Environment.GetEnvironmentVariable() +*/ privateKeyFolder + decryptorUser.Login + ".txt", aes.CreateDecryptor());
            }
            return null;

        }

        private string GetPublicKey(User user)
        {
            return FileToString(publicKeyFolder + user.Login + ".txt");
        }

        private byte[] GenerateKey(int size)
        {
            byte[] result = new byte[size];
            Random rnd = new Random((int)((DateTime.Now - DateTime.MinValue).TotalMilliseconds % int.MaxValue));
            for (int i=0; i<size; i++)
            {            
                byte n = (byte)rnd.Next(0, 255);
                result[i] = n;
            }
            return result;
        }

        private void AddUserAction(object obj)
        {
            byte[] password = GetHash(Password);
            if (CanAddUser(Login, Users))
            {
                RSA rsa = RSA.Create();
                string publicRsa = rsa.ToXmlString(false);
                StringToFile(publicKeyFolder + Login + ".txt", publicRsa);       

                string privateRsa = rsa.ToXmlString(true);
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = password;
                EncryptStringToFile(privateRsa, privateKeyFolder + Login + ".txt", aes.CreateEncryptor());

                Users.Add(new User(Login, password));
            }
            
        }

        private bool CanAddUser(string login, IEnumerable<User> Users)
        {
            foreach (User u in Users)
            {
                if (u.Login.Equals(Login)) return false;
            }
            return true;
        }

        private void DeleteUserAction(object obj)
        {
            for (int i = Users.Count - 1; i >= 0; i--)
            {
                if (Users[i].IsSelected) Users.RemoveAt(i);
            }
        }

        private void BrowseSourceFileAction(object obj)
        {
            var fileDialog = new OpenFileDialog();
            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SourceFile = fileDialog.FileName;
                FileInfo fi = new FileInfo(SourceFile);
                FileSize = fi.Length.ToString();
                OnPropertyChanged("ListBoxUsers");
                

            }
        }

        private void BrowseDestinationFolderAction(object obj)
        {
            var folderDialog = new FolderBrowserDialog();
            var result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DestinationFolder = folderDialog.SelectedPath;
            }
        }

        public void WriteHeader(string filePathOutput, Aes aes, List<User> recivers, string extension)
        {
            ApplicationStatus = "Zapisywanie metadanych";
            FileStream fs = new FileStream(filePathOutput, FileMode.Create);
            XmlWriter xmlWriter = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true, IndentChars = "\t", Encoding = Encoding.UTF8 });

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("EncryptedFileHeader");

            xmlWriter.WriteElementString("Algorithm", "AES");
            xmlWriter.WriteElementString("KeySize", aes.KeySize.ToString());
            xmlWriter.WriteElementString("BlockSize", aes.BlockSize.ToString());
            xmlWriter.WriteElementString("FeedbackSize", aes.FeedbackSize.ToString());
            xmlWriter.WriteElementString("CipherMode", aes.Mode.ToString());
            aes.Mode = aes.Mode == CipherMode.OFB ? CipherMode.CFB : aes.Mode;
            string iv = ByteArrayToNumberString(aes.IV, ' ');
            xmlWriter.WriteElementString("IV", iv);
            xmlWriter.WriteElementString("Format", extension);

            xmlWriter.WriteStartElement("ApprovedUsers");

                    foreach (User u in recivers)
                    {
                        xmlWriter.WriteStartElement("User");

                        xmlWriter.WriteElementString("Login", u.Login);
                        byte[] sessionKey = EncryptSesionKey(aes.Key, u);
                        xmlWriter.WriteElementString("SessionKey", ByteArrayToNumberString(sessionKey, ' ')); //zaszyfruj za pomocą RSA klucz któym szyfrujesz plik

                        xmlWriter.WriteEndElement();
                    }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            fs.Close();
        }
        public Dictionary<string, string> ReadHeader(string filePathInput)
        {
            string s;
            return ReadHeader(filePathInput, null, out s);
        }
        public Dictionary<string, string> ReadHeader(string filePathInput, Aes aes, out string extension)
        {
            extension = null;
            Dictionary<string, string> approvedUsers = new Dictionary<string, string>();
            try
            {
                if (aes == null) aes = Aes.Create(); //for not exceptions if aes is null - read only users
                ApplicationStatus = "Odczytywanie metadanych";               
                XmlReader xmlReader = XmlReader.Create(filePathInput, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment, IgnoreWhitespace = true, IgnoreProcessingInstructions = true, IgnoreComments = true });
                xmlReader.MoveToContent();
                xmlReader.Read(); //xml EncryptFileHeader
                xmlReader.ReadElementContentAsString(); //Algorithm
                aes.KeySize = xmlReader.ReadElementContentAsInt(); //Keysize
                aes.GenerateKey(); //TODO             
                aes.BlockSize = xmlReader.ReadElementContentAsInt(); //Blocksize 
                aes.FeedbackSize = xmlReader.ReadElementContentAsInt();
                aes.Mode = (CipherMode)Enum.Parse(typeof(CipherMode), xmlReader.ReadElementContentAsString()); //CipherMode
                aes.Mode = aes.Mode == CipherMode.OFB ? CipherMode.CFB : aes.Mode;    
                string iv = xmlReader.ReadElementContentAsString();
                byte[] ivAes = NumberStringToByteArray(iv, ' ');
                aes.IV = ivAes;
                extension = xmlReader.ReadElementContentAsString();


                while (xmlReader.Read() && ((xmlReader.NodeType != XmlNodeType.EndElement) || (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name != "EncryptedFileHeader")))
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "User")
                    {
                        xmlReader.Read();
                        string login = xmlReader.ReadElementContentAsString(); //Login
                        string sessionKey = xmlReader.ReadElementContentAsString(); //SessionKey
                        approvedUsers.Add(login, sessionKey);
                    }
                }

                xmlReader.Close();
                ApplicationStatus = "Metadane pliku odczytane";
            }
            catch (Exception e)
            {
                if (ApplicationMode == ApplicationModeEnum.Decrypt) ApplicationStatus = "Nie można odczytać nagłówka pliku";
            }
            return approvedUsers;
        }

        public async Task EncryptFileToFile(string filePathInput, string filePathOutput, ICryptoTransform encryptor)
        {
            ApplicationStatus = "Szyfrowanie";
            FileStream input = new FileStream(filePathInput, FileMode.Open);
            FileStream output = new FileStream(filePathOutput, FileMode.Append);
            CryptoStream crypt = new CryptoStream(output, encryptor, CryptoStreamMode.Write);

            byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
            output.Write(newLine, 0, newLine.Length);
            long filesize = fileSize;
            long did = 0;

            await Task.Run(() =>
            {
                int data;
                while ((data = input.ReadByte()) != -1)
                {
                    CryptProgress = (int)(((double)did / filesize) * 100);
                    crypt.WriteByte((byte)data);
                    did++;
                    
                }
            });

            crypt.Close();
            output.Close();
            input.Close();

        }

        public void EncryptStringToFile(string text, string filePathOutput, ICryptoTransform encryptor)
        {

            MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(text));
            FileStream output = new FileStream(privateKeyFolder + Login + ".txt", FileMode.Create);
            CryptoStream crypt = new CryptoStream(output, encryptor, CryptoStreamMode.Write);

            int data;
            while ((data = input.ReadByte()) != -1)
            {
                crypt.WriteByte((byte)data);
            }
            crypt.Close();
            output.Close();
            input.Close();
        }

        public string EncryptStringToString(string text, ICryptoTransform encryptor)
        {
            MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(text));
            
            CryptoStream crypt = new CryptoStream(input, encryptor, CryptoStreamMode.Write);
            StreamWriter output = new StreamWriter(crypt);

            output.Write(text);
            byte[] encrypted = input.ToArray();
            string result = Encoding.UTF8.GetString(encrypted);
            
            output.Close();
            crypt.Close();
            input.Close();
            return result;
        }



        public async Task DecryptFileToFile(string filePathInput, string filePathOutput, ICryptoTransform decryptor)
        {




            FileStream input = new FileStream(filePathInput, FileMode.Open);
            ApplicationStatus = "Deszyfrowanie";
            StreamReader sr = new StreamReader(input);
            long pos = 0;
            string line;
            do
            {
                line = sr.ReadLine();
                pos += line.Length +2;
                Console.WriteLine(line + " " + pos);
            } while (line != "</EncryptedFileHeader>");
            input.Position = pos+ApprovedUsers.Count;

            //Console.WriteLine((char)input.ReadByte());
            //Console.WriteLine((char)input.ReadByte());
            //Console.WriteLine((char)input.ReadByte());
            //Console.WriteLine(input.ReadByte());


            //input.Position -= 4;
            //sr.Close();



            int data;
            CryptoStream crypto = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            FileStream output = new FileStream(filePathOutput, FileMode.Create);
            //for (int i = 0; i < 1000; i++) {
            //    int b = input.ReadByte();
            //    Console.Write(b + "(" + (char)b + ")");
            //}

            for (int i = 0; i < Encoding.ASCII.GetBytes(Environment.NewLine).Length; i++)
                data = input.ReadByte();



            long filesize = fileSize;
            long did = 0;

            await Task.Run(() =>
            {
                
                while ((data = crypto.ReadByte()) != -1)
                {
                    CryptProgress = (int)(((double)did / filesize) * 100);
                    output.WriteByte((byte)data);
                    did++;
                }
            });

            output.Close();
            crypto.Close();
            input.Close();
        }

        public string DecryptStringToString(string text, ICryptoTransform decryptor)
        {
            string result;
            MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(text));
            CryptoStream crypto = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            StreamReader output = new StreamReader(crypto, Encoding.UTF8);

            result = output.ReadToEnd();

            output.Close();
            crypto.Close();
            input.Close();

            return result;
        }

        public string DecryptFileToString(string filePathInput, ICryptoTransform decryptor)
        {
            string result;
            FileStream input = new FileStream(filePathInput, FileMode.Open);
            CryptoStream crypto = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            StreamReader output = new StreamReader(crypto, Encoding.UTF8);

            result = output.ReadToEnd();

            output.Close();
            crypto.Close();
            input.Close();

            return result;

        }

        public byte[] GetHash(string s)
        {
            SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        }

        public List<User> GetSelectedUsers()
        {
            List<User> result = new List<User>();
            foreach (User u in Users)
            {
                if (u.IsSelected) result.Add(u);
            }
            return result;
        }

        private bool ExportUsersXML(string filePathOutput, ObservableCollection<User> users)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(users.GetType());
                using (FileStream fs = new FileStream(filePathOutput, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {
                        serializer.Serialize(sw, users);
                        return true;
                    }
                }
            }
            catch { return false; }
        }

        private bool ImportUsersXML(string filePathInput, ObservableCollection<User> users)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(Users.GetType());
                using (FileStream fs = new FileStream(filePathInput, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {
                        ObservableCollection<User> usersRead = (ObservableCollection<User>)serializer.Deserialize(sr);
                        
                        foreach (User u in usersRead)
                        {

                            //string privateRsa = DecryptPrivateRsa(u);
                            //using (FileStream fsRsa = new FileStream("Prywatne/" + u.Login, FileMode.Open))
                            //{
                            //    using (StreamReader swRsa = new StreamReader(fsRsa))
                            //    {
                            //        privateRsa = swRsa.ReadToEnd();
                            //    }
                            //}
                            //RSA rsa = RSA.Create();
                            //rsa.FromXmlString(privateRsa);
                            //u.Rsa = rsa;
                            users.Add(u);
                        }
                        return true;
                    }
                }
            }
            catch { return false; }
        }

        private void StringToFile(string filePathOutput, string content)
        {
            using (FileStream fs = new FileStream(filePathOutput, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(content);
                }
            }
        }

        private string FileToString(string filePathInput)
        {
            string result;
            using (FileStream fs = new FileStream(filePathInput, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        private string ByteArrayToNumberString(byte[] bytes, char delimeter)
        {
            string result = "";
            foreach (byte b in bytes)
            {
                result += (int)b + " ";
            }
            return result;
        }

        private byte[] NumberStringToByteArray(string text, char delimeter)
        {     
            string[] textSplitted = text.Split(delimeter);
            byte[] bytes = new byte[textSplitted.Length - 1];
            for (int i = 0; i < textSplitted.Length-1; i++) bytes[i] = byte.Parse(textSplitted[i].ToString());
            return bytes;
        }

    }

    
}


/*
 * 
 * https://www.codeproject.com/Articles/26085/File-Encryption-and-Decryption-in-C
 * 
 * 
 * 
 * 
 * 
 */
