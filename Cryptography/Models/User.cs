﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.Models
{
    public class User
    {
        public string Login { get; set; }
        private string login;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }
        private bool isSelected;

        public User(string login)
        {
            Login = login;
        }

    }


}
