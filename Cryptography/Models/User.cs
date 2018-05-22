using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Cryptography.Models
{
    [Serializable]
    public class User
    {
        public string Login { get; set; }
        public byte[] Password { get; set; }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }
        private bool isSelected;

        public User() { }
        public User(string login, byte[] password=null)
        {
            Login = login;
            Password = password;
            IsSelected = false;
        }

        public override bool Equals(object obj)
        {
            User objUser = obj as User;
            if (objUser == null) return false;
            return this.Login == objUser.Login;
        }



    }


}
