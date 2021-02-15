using AstroWebComp.Helper;
using AstroWebComp.Models;
using AstroWebComp.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AstroWebComp.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            ViewBag.Title = "Admin Login";

            return View();
        }
        public ActionResult Home()
        {
            ViewBag.Title = "Admin Login";

            return View();
        }
        public ActionResult Logout()
        {
            Session.Remove("UserName");
            ViewBag.Title = "Admin Login";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<ActionResult> Index(LoginViewModel model)
        {
            var jsonObj = JsonConvert.SerializeObject(model);
            var client = new HttpClient();
            StringContent stringContent = new StringContent(jsonObj, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Urls.LOGIN, stringContent).Result.Content.ReadAsStringAsync();
            WebResponse adminLoginResponse = JsonConvert.DeserializeObject<WebResponse>(response);
            if (!adminLoginResponse.error)
            {
                Session["UserName"] = adminLoginResponse.data.ToString();
                return RedirectToAction("Details", "Home");
            }
            else
            {
                ViewData["ErrorMessage"] = "Error trying to login , please try again.";
                return View();
            }
        }
    }
}