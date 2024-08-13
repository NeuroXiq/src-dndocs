using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Admin
{
    public class AdminLoginModel
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public AdminLoginModel() { }
        
        public AdminLoginModel(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}
