using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AstroWebComp.Helper
{
    public class Urls
    {
        public const string BASE_URL = "http://103.16.222.196/api/";
        //public const string BASE_URL = "http://localhost:52275/api/";
        public const string LOGIN = BASE_URL + "admin/login";
        public const string GET_USERS = BASE_URL + "admin/login";
        public const string GET_COUPONS = BASE_URL + "admin/GetCoupons";
        public const string MODIFY_COUPONS = BASE_URL + "admin/ModifyCoupon";
        public const string CREATE_COUPONS = BASE_URL + "admin/CreateCoupon";
        public const string DELETE_COUPONS = BASE_URL + "admin/DeleteCoupon";
        public const string GET_USER_DETAILS = BASE_URL + "Admin/GetUserDetails";
    }
}