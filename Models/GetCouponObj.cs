using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StratBazWebComp.Models
{
    public class GetCouponObj
    {
        public int id { get; set; }
        public string name { get; set; }
        public double percent { get; set; }
        public short active { get; set; }
        public string Datetime { get; set; }
        public string module { get; set; }
    }
}