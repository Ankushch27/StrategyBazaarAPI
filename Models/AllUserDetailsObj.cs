using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AstroWebComp.Models
{
    public class AllUserDetailsObj
    {
        public string Timestamp { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Module { get; set; }
        public string Expiry { get; set; }
        public string LastAmountPaid { get; set; }
        public string DateModified { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", Timestamp, Username, Email, Mobile, (Module == "1" ? "Free" : Module == "2" ? "Intraday" : "Positional"), Expiry, LastAmountPaid, DateModified);
        }
    }
}