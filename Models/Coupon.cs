using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AstroWebComp.Models
{
    public class Coupon
    {
        public string CouponName { get; set; }
        public string Percent { get; set; }
        public int isActive { get; set; }
        public string Module { get; set; }
    }
}