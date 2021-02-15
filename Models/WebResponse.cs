using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StratBazWebComp.Models
{
    public class WebResponse
    {
        public string message { get; set; }
        public int code { get; set; }
        public bool error { get; set; }
        public object data { get; set; }
        public string status { get; set; }
    }

}