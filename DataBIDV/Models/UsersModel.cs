using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Models
{
    class UsersModel
    {
    }
    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string Permission { get; set; }
        public string Source { get; set; }
        public string DisplayName { get; set; }
        public bool Status { get; set; }
        public string Code { get; set; }
    }
    public class Users
    {
        public string UserID { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
        public string Code { get; set; }

    }
}
