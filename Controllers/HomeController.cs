using StratBazWebComp.Helper;
using StratBazWebComp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace StratBazWebComp.Controllers
{
    [CustomAuthFilter]
    public class HomeController : Controller
    {
        public class WebOutput
        {
            public bool OK { get; set; }
            public object data { get; set; }
            public string message { get; set; }
            public List<string> errorField { get; set; }
        }
        public ActionResult Details()
        {
            return View();
        }
        public ActionResult Coupon_Mgmt()
        {
            return View();
        }

        public async Task<string> GetAllDetails()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Session["UserName"]);
            var response = await client.GetAsync(Urls.GET_USER_DETAILS).Result.Content.ReadAsStringAsync();

            var WebResponse = JsonConvert.DeserializeObject<WebOutput>(response);
            //AdminFeaturesResponse adminFeaturesResponse = JsonConvert.DeserializeObject<AdminFeaturesResponse>(FeaturesResponse);

            return string.IsNullOrEmpty(WebResponse.data.ToString()) ? "" : WebResponse.data.ToString();
        }

        public async Task<string> GetCoupons()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Session["UserName"]);
            var response = await client.GetAsync(Urls.GET_COUPONS).Result.Content.ReadAsStringAsync();

            var WebResponse = JsonConvert.DeserializeObject<WebOutput>(response);
            //AdminFeaturesResponse adminFeaturesResponse = JsonConvert.DeserializeObject<AdminFeaturesResponse>(FeaturesResponse);

            return string.IsNullOrEmpty(WebResponse.data.ToString()) ? "" : WebResponse.data.ToString();
        }

        public async Task<string> ModifyCoupon(string name, double percent, bool active, string license, int id = 0)
        {
            if (id == 0)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Session["UserName"]);
                var response = await client.PostAsync(Urls.CREATE_COUPONS, new StringContent(JsonConvert.SerializeObject(new Coupon
                {
                    CouponName = name,
                    isActive = active ? 1 : 0,
                    Percent = percent.ToString(),
                    Module = license == "3" ? "2," + license : license
                }), Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync();

                var WebResponse = JsonConvert.DeserializeObject<WebOutput>(response);
                //AdminFeaturesResponse adminFeaturesResponse = JsonConvert.DeserializeObject<AdminFeaturesResponse>(FeaturesResponse);

                return string.IsNullOrEmpty(WebResponse.data.ToString()) ? "" : WebResponse.data.ToString();

            }
            else
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Session["UserName"]);
                var response = await client.PostAsync(Urls.MODIFY_COUPONS, new StringContent(JsonConvert.SerializeObject(new Coupon { CouponName = name, isActive = active ? 1 : 0, Percent = percent.ToString(), Module = license == "3" ? "2," + license : license }), Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync();

                var WebResponse = JsonConvert.DeserializeObject<WebOutput>(response);
                //AdminFeaturesResponse adminFeaturesResponse = JsonConvert.DeserializeObject<AdminFeaturesResponse>(FeaturesResponse);

                return string.IsNullOrEmpty(WebResponse.data.ToString()) ? "" : WebResponse.data.ToString();
            }
        }

        public async Task<string> DeleteCoupon(string name)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Session["UserName"]);
            var response = await client.DeleteAsync(Urls.DELETE_COUPONS + "/" + name).Result.Content.ReadAsStringAsync();

            var WebResponse = JsonConvert.DeserializeObject<WebOutput>(response);
            //AdminFeaturesResponse adminFeaturesResponse = JsonConvert.DeserializeObject<AdminFeaturesResponse>(FeaturesResponse);

            return string.IsNullOrEmpty(WebResponse.data.ToString()) ? "" : WebResponse.data.ToString();
        }
        public async Task<FileContentResult> DownloadCSV()
        {
            var result = await GetAllDetails();
            var adminUsersResponse = JsonConvert.DeserializeObject<List<AllUserDetailsObj>>(result);
            var csvHeaders = "Timestamp,Username,Email,Mobile,Module,Expiry,LastAmountPaid,DateModified\n";
            return File(new System.Text.UTF8Encoding().GetBytes(string.Join(csvHeaders + "\n", adminUsersResponse)), "text/csv", string.Format("{0}{1}.csv", "UsersList ", DateTime.Now.ToString("d")));
        }

    }
}