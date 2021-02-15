using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.IO;
using System.Threading.Tasks;
using AstroWebComp.Models;
using WebResponse = AstroWebComp.Models.WebResponse;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using EASendMail;
using System.Threading;
using System.Net.Mail;
using SmtpClient = System.Net.Mail.SmtpClient;
using MailAddress = System.Net.Mail.MailAddress;

namespace AstroWebComp.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        public MySqlConnection sqlConn = new MySqlConnection(
            "server=103.16.222.196;user id=root;database=astro;password=j/vYN(6KL(;port=3306");

        private static List<ValidateToken> tokens = new List<ValidateToken>();
        public class ValidateToken
        {
            public string user { get; set; }
            public DateTime lastLogin { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult Login(Login login)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                Console.WriteLine("Connecting to MySQL...");
                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_User_Login";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@pass", login.Password);
                cmd.Parameters["@pass"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@mob", login.Mobile);
                cmd.Parameters["@mob"].Direction = ParameterDirection.Input;
                int UserID = 0;
                using (var cursor = cmd.ExecuteReader())
                {

                    if (cursor.Read())
                    {
                        var userid = Convert.ToInt32(cursor["id"]);
                        var expiry = Convert.ToDateTime(cursor["expiry"]);
                        sqlConn.Close();
                        if (expiry.Date < DateTime.Now.Date)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Error",
                                data = "License expired",
                                error = false
                            });
                        }
                        UserID = userid;
                    }
                    else
                    {
                        sqlConn.Close();
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.BadRequest,
                            status = "Not_Ok",
                            message = "Error",
                            data = "Login failed",
                            error = false
                        });
                    }
                }



                string key = "5a127994a9352fdbf6e045f4bfd80884"; //Secret key which will be used later during validation    
                var issuer = "AstroTrading";  //normally this will be your site URL    

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                //Create a List of Claims, Keep claims name short    
                var permClaims = new List<Claim>();
                permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                permClaims.Add(new Claim("valid", "1"));
                permClaims.Add(new Claim("userid", UserID.ToString()));
                var dt = DateTime.Now;
                permClaims.Add(new Claim("LastLogin", dt.ToString()));

                var idx = tokens.FindIndex(x => x.user == UserID.ToString());
                if (idx == -1)
                {
                    tokens.Add(new ValidateToken { lastLogin = dt, user = UserID.ToString() });
                }
                else
                {
                    tokens[idx].lastLogin = dt;
                }



                //Create Security Token object by giving required parameters    
                var token = new JwtSecurityToken(issuer, //Issure    
                                issuer,  //Audience    
                                permClaims,
                                expires: DateTime.Now.AddDays(1),
                                signingCredentials: credentials);
                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = jwt_token,
                    error = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpGet]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult GetDetails()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                    var userid = claims.Where(p => p.Type == "userid").FirstOrDefault()?.Value;
                    var lastLogin = claims.Where(p => p.Type == "LastLogin").FirstOrDefault()?.Value;
                    if (userid != null)
                    {
                        var idx = tokens.FindIndex(x => x.user == userid);
                        if (idx == -1)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }
                        if (tokens[idx].lastLogin.ToString() != lastLogin)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }

                        sqlConn.Open();
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = sqlConn;

                        cmd.CommandText = "spo_Get_User_Details";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@user_id", userid);
                        cmd.Parameters["@user_id"].Direction = ParameterDirection.Input;


                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            status = "Ok",
                            message = "Success",
                            data = dt,
                            error = false
                        });
                    }
                }
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.BadRequest,
                    status = "Not_Ok",
                    message = "Success",
                    data = "UnAuthorized",
                    error = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpGet]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult GetData5m(string id)
        {
            string url = $"http://103.240.90.197/{id}.jpg";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                    var userid = claims.Where(p => p.Type == "userid").FirstOrDefault()?.Value;
                    var lastLogin = claims.Where(p => p.Type == "LastLogin").FirstOrDefault()?.Value;
                    if (userid != null)
                    {
                        var idx = tokens.FindIndex(x => x.user == userid);
                        if (idx == -1)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }
                        if (tokens[idx].lastLogin.ToString() != lastLogin)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }

                        var stream = new MemoryStream();
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            status = "Ok",
                            message = "Success",
                            data = System.Convert.ToBase64String(new WebClient().DownloadData(url)),
                            error = false
                        });
                    }
                }
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.BadRequest,
                    status = "Not_Ok",
                    message = "Success",
                    data = "UnAuthorized",
                    error = false
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        status = "Not_Ok",
                        message = "Error",
                        data = "Invalid Requested Filename!!",
                        error = false
                    });
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpGet]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult GetData15m(string id)
        {
            string url = $"http://103.240.90.127/{id}.jpg";
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                    var userid = claims.Where(p => p.Type == "userid").FirstOrDefault()?.Value;
                    var lastLogin = claims.Where(p => p.Type == "LastLogin").FirstOrDefault()?.Value;
                    if (userid != null)
                    {
                        var idx = tokens.FindIndex(x => x.user == userid);
                        if (idx == -1)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }
                        if (tokens[idx].lastLogin.ToString() != lastLogin)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }

                        var stream = new MemoryStream();
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            status = "Ok",
                            message = "Success",
                            data = System.Convert.ToBase64String(new WebClient().DownloadData(url)),
                            error = false
                        });
                    }
                }
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.BadRequest,
                    status = "Not_Ok",
                    message = "Success",
                    data = "UnAuthorized",
                    error = false
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        status = "Not_Ok",
                        message = "Error",
                        data = "Invalid Requested Filename!!",
                        error = false
                    });
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpGet]
        [ResponseType(typeof(WebResponse))]
        public async Task<IHttpActionResult> GetCSV(string id)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                    var userid = claims.Where(p => p.Type == "userid").FirstOrDefault()?.Value;
                    var lastLogin = claims.Where(p => p.Type == "LastLogin").FirstOrDefault()?.Value;
                    if (userid != null)
                    {
                        var idx = tokens.FindIndex(x => x.user == userid);
                        if (idx == -1)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }
                        if (tokens[idx].lastLogin.ToString() != lastLogin)
                        {
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = "Login Expired!",
                                error = false
                            });
                        }

                        List<string> lines = new List<string>();
                        string WL = "";
                        switch (id)
                        {
                            case "WLMD":
                                WL = "c:\\AAT\\MONEY MonSoon Daily.CSV";
                                break;
                            case "WLMH":
                                WL = "c:\\AAT\\MONEY MonSoon Hourly.CSV";
                                break;
                            case "WLMW":
                                WL = "c:\\AAT\\MONEY MonSoon Weekly.CSV";
                                break;
                            case "WLM15":
                                WL = "c:\\AAT\\MONEY MonSoon 15MIN.CSV";
                                break;
                            case "WL15":
                                WL = "c:\\AAT\\MONEY MonSoon 15MIN.CSV";
                                break;
                            default:
                                WL = "c:\\AAT\\MONEY ATM 5MIN.CSV";
                                break;
                        }
                        try
                        {
                            var fs = new FileStream(WL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            var stream = new StreamReader(fs);
                            while (!stream.EndOfStream)
                                lines.Add(await stream.ReadLineAsync());
                            stream.Close();
                            fs.Close();
                        }
                        catch (Exception ex)
                        {
                            WL = ex.Message;
                        }
                        if (lines.Count > 59)
                        {

                            var sorted = lines.Select(line => new
                            {
                                SortKey = line.Split(',')[0],
                                Line = line
                            }).OrderBy(x => x.SortKey).Select(x => x.Line);
                            if (sorted.ToList()[0].Split(',').Length < 35) sorted = sorted.Skip(1);

                            WL = string.Join("\r\n", sorted);
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.OK,
                                status = "Ok",
                                message = "Success",
                                data = new { Watch = WL },
                                error = false
                            });
                        }
                        else
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Success",
                                data = new { Watch = "Not available" },
                                error = false
                            });
                    }
                }
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.BadRequest,
                    status = "Not_Ok",
                    message = "Success",
                    data = "UnAuthorized",
                    error = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult RegisterUser(Register register)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Update_User";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@username", register.Username);
                cmd.Parameters["@username"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@email", register.Email);
                cmd.Parameters["@email"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@password", register.Password);
                cmd.Parameters["@password"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@mob", register.Mobile);
                cmd.Parameters["@mob"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@isUpdate", register.Update);
                cmd.Parameters["@isUpdate"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@new_Lic", register.Lic_update);
                cmd.Parameters["@new_Lic"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@new_module", register.Module);
                cmd.Parameters["@new_module"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Lic", register.NumberOfLicenses);
                cmd.Parameters["@Lic"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@AMT", register.Amount);
                cmd.Parameters["@AMT"].Direction = ParameterDirection.Input;

                var check = cmd.ExecuteNonQuery();
                sqlConn.Close();
                if (!register.Update && check == 0)
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        status = "Not_Ok",
                        message = "Error",
                        data = "User already registered!!",
                        error = false
                    });
                if (register.Update || register.Lic_update)
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        status = "Ok",
                        message = "Success",
                        data = "User is Updated!!",
                        error = false
                    });
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = "User is Registered!!",
                    error = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult TransactionUpdate(TransactionLog Log)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Update_License";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Mobile", Log.Mobile);
                cmd.Parameters["@Mobile"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@receipt_no", Log.Receipt_no);
                cmd.Parameters["@receipt_no"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@amt", Log.Amount);
                cmd.Parameters["@amt"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@order_no", Log.Order_no);
                cmd.Parameters["@order_no"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Trans", Log.Trans);
                cmd.Parameters["@Trans"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@payment_no", Log.Payment_no);
                cmd.Parameters["@payment_no"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@sign", Log.Sign);
                cmd.Parameters["@sign"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@isSuccess", Log.isSuccess);
                cmd.Parameters["@isSuccess"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Coupon", Log.Coupon);
                cmd.Parameters["@Coupon"].Direction = ParameterDirection.Input;

                var check = cmd.ExecuteNonQuery();
                sqlConn.Close();
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = "Log Updated!!",
                    error = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpPost]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult GetCouponValue(CouponValue coupon)
        {

            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Get_Coupon_Value";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CouponName", coupon.CouponName);
                cmd.Parameters["@CouponName"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Mobile", coupon.Mobile);
                cmd.Parameters["@Mobile"].Direction = ParameterDirection.Input;

                using (var cursor = cmd.ExecuteReader())
                {

                    if (cursor.Read())
                    {
                        var value = cursor[0];
                        if (decimal.TryParse(value.ToString(), out decimal dval))
                        {
                            var modules = cursor[1];
                            sqlConn.Close();

                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.OK,
                                status = "Ok",
                                message = "Success",
                                data = string.Format("{0}:{1}", value, modules),
                                error = false
                            });

                        }
                        else
                        {
                            sqlConn.Close();
                            return Ok(new WebResponse
                            {
                                code = (int)HttpStatusCode.BadRequest,
                                status = "Not_Ok",
                                message = "Error",
                                data = value,
                                error = true
                            });
                        }
                    }
                    else
                    {
                        sqlConn.Close();
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.BadRequest,
                            status = "Not_Ok",
                            message = "Error",
                            data = "Coupon is not vaild!",
                            error = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult UpdatePassword(UpdatePassword details)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Update_Password";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@new_pass", details.Password);
                cmd.Parameters["@new_pass"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@mob", details.Mobile);
                cmd.Parameters["@mob"].Direction = ParameterDirection.Input;

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        status = "Ok",
                        message = "Success",
                        data = "Password is updated Successfully",
                        error = false
                    });
                }

                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.BadRequest,
                    status = "Not_Ok",
                    message = "Success",
                    data = "User not Exist",
                    error = true
                });

            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult RequestOtp(RequestOTPObj obj)
        {
            try
            {
                #region Smtp old
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("dhaval@authenticastrotrading.com");
                    //if (EmailToAddress.Contains(";"))
                    //{
                    //    string[] emails = EmailToAddress.Split(";".ToCharArray());
                    //    foreach (string e in emails)
                    //    {
                    //        mail.To.Add(e);
                    //    }
                    //}
                    //else
                    //{
                    mail.To.Add(obj.To);
                    //}
                    if (obj.isRegister)
                    {
                        mail.Subject = "OTP for Registration";
                        mail.Body = "<div>Hi  <br>Your OTP is <b>{OTP}</b> required to complete your registration.<br><b id=\"" +
                            "docs-internal-guid-5cc09dfe-7fff-c975-81f1-c26d56cfaafd\" style=\"font-weight:normal;\"><span class=\"highlight\"" +
                            " style=\"background-color:transparent\"><span class=\"colour\" style=\"color:rgb(0, 0, 0)\"><span class=\"font\" " +
                            "style=\"font-family:Arial\"><span class=\"size\" style=\"font-size: 11pt; font-weight: 400; font-style: normal;" +
                            " font-variant: normal; text-decoration: none; vertical-align: baseline; white-space: pre-wrap;\">Please enter this" +
                            " OTP on the request screen to complete process.</span></span></span></span></b></div><div><br></div><div>Regards,<br>Authentic Astro Trading<br></div>";
                    }
                    else
                    {
                        mail.Subject = "OTP for Password Reset";
                        mail.Body = "<div>Hi <br>Your OTP is <b>{OTP}</b> required to reset your password.</div><div><br></div><div>Regards,<br>Authentic Astro Trading<br></div>";
                    }
                    string Otp = new Random().Next(100000, 999999).ToString();

                    mail.Body = mail.Body.Replace("{OTP}", Otp);
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("dhaval@authenticastrotrading.com", "123456789");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }

                    string key = "5a127994a9352fdbf6e045f4bfd80884"; //Secret key which will be used later during validation    
                    var issuer = "AstroTrading";  //normally this will be your site URL    

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    //Create a List of Claims, Keep claims name short    
                    var permClaims = new List<Claim>();
                    permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                    permClaims.Add(new Claim("valid", "1"));
                    permClaims.Add(new Claim("otp", Otp));

                    //Create Security Token object by giving required parameters    
                    var token = new JwtSecurityToken(issuer, //Issure    
                                    issuer,  //Audience    
                                    permClaims,
                                    expires: DateTime.Now.AddHours(3),
                                    signingCredentials: credentials);
                    var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        status = "Ok",
                        message = "Mail Send Successfully!!",
                        data = jwt_token,
                        error = false
                    });
                }

                #endregion

            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

        [HttpPost]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult VerifyOTP(VerifyOTPObj verify)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var otp = claims.Where(p => p.Type == "otp").FirstOrDefault()?.Value;
                if (otp != null)
                    if (verify.OTP == otp)
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.OK,
                            status = "Ok",
                            message = "Success",
                            data = "OTP is correct!",
                            error = false
                        });
                    else
                        return Ok(new WebResponse
                        {
                            code = (int)HttpStatusCode.BadRequest,
                            status = "Not_Ok",
                            message = "Failure",
                            data = "OTP is not correct",
                            error = true
                        });
            }
            return Ok(new WebResponse
            {
                code = (int)HttpStatusCode.BadRequest,
                status = "Not_Ok",
                message = "Failure",
                data = "UnAuthorized",
                error = true
            });
        }
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof(WebResponse))]
        public async Task<IHttpActionResult> GetFreeCSV([FromUri] string id)
        {
            try
            {
                List<string> lines = new List<string>();
                string WL = "";
                switch (id)
                {
                    case "WLMD":
                        WL = "c:\\AAT\\MONEY MonSoon Daily.CSV";
                        break;
                    case "WLMH":
                        WL = "c:\\AAT\\MONEY MonSoon Hourly.CSV";
                        break;
                    case "WLMW":
                        WL = "c:\\AAT\\MONEY MonSoon Weekly.CSV";
                        break;
                    case "WLM15":
                        WL = "c:\\AAT\\MONEY MonSoon 15MIN.CSV";
                        break;
                    case "WL15":
                        WL = "c:\\AAT\\MONEY MonSoon 15MIN.CSV";
                        break;
                    default:
                        WL = "c:\\AAT\\MONEY ATM 5MIN.CSV";
                        break;
                }
                try
                {
                    var fs = new FileStream(WL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var stream = new StreamReader(fs);
                    while (!stream.EndOfStream)
                        lines.Add(await stream.ReadLineAsync());
                    stream.Close();
                    fs.Close();
                }
                catch (Exception ex)
                {
                    WL = ex.Message;
                }
                if (lines.Count > 59)
                {

                    var sorted = lines.Select(line => new
                    {
                        SortKey = line.Split(',')[0],
                        Line = line
                    }).OrderBy(x => x.SortKey).Select(x => x.Line);
                    if (sorted.ToList()[0].Split(',').Length < 35) sorted = sorted.Skip(1);

                    WL = string.Join("\r\n", sorted);
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.OK,
                        status = "Ok",
                        message = "Success",
                        data = new { Watch = WL },
                        error = false
                    });
                }
                else
                    return Ok(new WebResponse
                    {
                        code = (int)HttpStatusCode.BadRequest,
                        status = "Not_Ok",
                        message = "Success",
                        data = new { Watch = "Not available" },
                        error = false
                    });
            }
            catch (Exception ex)
            {
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.InternalServerError,
                    status = "Not_Ok",
                    message = "Error",
                    data = ex.Message,
                    error = true
                });
            }
        }

    }
}
