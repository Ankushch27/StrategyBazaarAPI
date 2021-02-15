using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AstroWebComp.Models
{
    public class TransactionLog
    {
        public string Mobile { get; set; } = "";
        public string Receipt_no { get; set; } = "";
        public string Amount { get; set; } = "";
        public string Order_no { get; set; } = "";
        public string Trans { get; set; } = "";
        public string Payment_no { get; set; } = "";
        public string Sign { get; set; } = "";
        public string Coupon { get; set; } = "";
        public int isSuccess { get; set; } = 0;
    }
}