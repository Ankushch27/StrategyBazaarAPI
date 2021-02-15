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
using StratBazWebComp.Models;
using WebResponse = StratBazWebComp.Models.WebResponse;

namespace StratBazWebComp.Controllers
{
    [Authorize]
    public class AdminController : ApiController
    {
        public MySqlConnection sqlConn = new MySqlConnection(
           "server=103.16.222.196;user id=root;database=strategy_bazaar;password=j/vYN(6KL(;port=3306");
        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult Login(Admin login)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                Console.WriteLine("Connecting to MySQL...");
                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = " SELECT * FROM `admin` WHERE name = @admin AND password = @pass; ";

                cmd.Parameters.AddWithValue("@pass", login.Password);
                cmd.Parameters["@pass"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@admin", login.Username);
                cmd.Parameters["@admin"].Direction = ParameterDirection.Input;
                int AdminID = 0;
                using (var cursor = cmd.ExecuteReader())
                {

                    if (cursor.Read())
                    {
                        var userid = Convert.ToInt32(cursor["id"]);
                        sqlConn.Close();
                        AdminID = userid;
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
                            error = true
                        });
                    }
                }

                string key = "5a127994a9352fdbf6e045f4bfd80884"; //Secret key which will be used later during validation    
                var issuer = "StratBaz";  //normally this will be your site URL    

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                //Create a List of Claims, Keep claims name short    
                var permClaims = new List<Claim>();
                permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                permClaims.Add(new Claim("valid", "1"));
                permClaims.Add(new Claim("Adminid", AdminID.ToString()));
                var dt = DateTime.Now;

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
        public IHttpActionResult GetUserDetails()
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                Console.WriteLine("Connecting to MySQL...");
                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Get_All_Users_Detail";
                cmd.CommandType = CommandType.StoredProcedure;
                List<AllUserDetailsObj> list = new List<AllUserDetailsObj>();
                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        list.Add(new AllUserDetailsObj
                        {
                            Timestamp = cursor["create_time"].ToString(),
                            DateModified = cursor["date_modiified"].ToString(),
                            Email = cursor["email"].ToString(),
                            Expiry = cursor["expiry"].ToString(),
                            Password = cursor["password"].ToString(),
                            Mobile = cursor["mobile"].ToString(),
                            LastAmountPaid = cursor["last_amount"].ToString(),
                            Module = cursor["module"].ToString(),
                            Username = cursor["username"].ToString()
                        });
                    }
                    sqlConn.Close();
                }

                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = list,
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
        public IHttpActionResult GetCoupons()
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                Console.WriteLine("Connecting to MySQL...");
                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Get_Coupons";
                cmd.CommandType = CommandType.StoredProcedure;
                List<GetCouponObj> list = new List<GetCouponObj>();
                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        list.Add(new GetCouponObj
                        {
                            id = Convert.ToInt32(cursor["id"].ToString()),
                            active = Convert.ToInt16(cursor["active"].ToString()),
                            Datetime = cursor["Datetime"].ToString(),
                            module = cursor["module"].ToString(),
                            name = cursor["name"].ToString(),
                            percent = Convert.ToDouble(cursor["percent"].ToString())
                        });
                    }
                    sqlConn.Close();
                }

                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = list,
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
        public IHttpActionResult CreateCoupon(Coupon coupon)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Create_Coupon";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CouponName", coupon.CouponName);
                cmd.Parameters["@CouponName"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Percent", coupon.Percent);
                cmd.Parameters["@Percent"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@isActive", coupon.isActive);
                cmd.Parameters["@isActive"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Module", coupon.Module);
                cmd.Parameters["@Module"].Direction = ParameterDirection.Input;

                var check = cmd.ExecuteNonQuery();
                sqlConn.Close();
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = "New Coupon Added!!",
                    error = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new Models.WebResponse
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
        public IHttpActionResult ModifyCoupon(Coupon coupon)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Modify_Coupon";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CouponName", coupon.CouponName);
                cmd.Parameters["@CouponName"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Percent", coupon.Percent);
                cmd.Parameters["@Percent"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@isActive", coupon.isActive);
                cmd.Parameters["@isActive"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Module", coupon.Module);
                cmd.Parameters["@Module"].Direction = ParameterDirection.Input;

                var check = cmd.ExecuteNonQuery();
                sqlConn.Close();
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = "Coupon Modified!!",
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

        [HttpDelete]
        [ResponseType(typeof(WebResponse))]
        public IHttpActionResult DeleteCoupon(string id)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();

                sqlConn.Open();
                cmd.Connection = sqlConn;

                cmd.CommandText = "spo_Delete_Coupon";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CouponName", id);
                cmd.Parameters["@CouponName"].Direction = ParameterDirection.Input;

                var check = cmd.ExecuteNonQuery();
                sqlConn.Close();
                return Ok(new WebResponse
                {
                    code = (int)HttpStatusCode.OK,
                    status = "Ok",
                    message = "Success",
                    data = "Coupon Deleted!!",
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
