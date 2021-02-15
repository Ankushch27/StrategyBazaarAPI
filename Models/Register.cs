using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StratBazWebComp.Models
{
    public class Register
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Amount { get; set; } = "0";
        public string Mobile { get; set; }
        public bool Update { get; set; }
        public bool Lic_update { get; set; }
        public int Module { get; set; }
        public int NumberOfLicenses { get; set; }
    }
}