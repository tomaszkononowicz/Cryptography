using Cryptography.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties
        public string Login { get; set; }
        public string Password { get; set; }
        public string DestinationFolder { get; set; }
        public string OutputFilename { get; set; }
        public string SourceFile { get; set; }
        public string FileSize { get; set; }
        public string ApplicationStatus { get; set; }
        public int CryptProgress { get; set; }

        private int applicationMode;
        public int ApplicationMode
        {
            get { return applicationMode; }
            set
            {
                if (value != 0)
                    applicationMode = value;
                OnPropertyChanged("ApplicationMode");
            }
        }

        private int cryptBlockMode;
        public int CryptBlockMode
        {
            get { return cryptBlockMode; }
            set
            {
                if (value != 0)
                    cryptBlockMode = value;
                OnPropertyChanged("ApplicationMode");
            }
        }

        #endregion


        public ObservableCollection<User> Users { get; set; }

        public MainViewModel()
        {
            Users = new ObservableCollection<User>();
            Login = "Login";
            Password = "Haslo";
            Users.Add(new User("Użytkownik 1"));
            Users.Add(new User("Użytkownik 2"));
            Users.Add(new User("Użytkownik 3"));
            ApplicationMode = 1;
            CryptBlockMode = 1;
            SourceFile = "C:\\Users\\Tomasz\\Desktop\\Projek1 - dokumentacja";
            DestinationFolder = "C:\\Pulpit";
            OutputFilename = "Przerobiony";          
            FileSize = "124 MB";
            ApplicationStatus = "Deszyfrowanie";
            CryptProgress = 28;


        }

    }
}
