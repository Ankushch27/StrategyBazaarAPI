using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;


[assembly: OwinStartup(typeof(AstroWebComp.Startup))]

namespace AstroWebComp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "AstroTrading", //some string, normally web url,  
                        ValidAudience = "AstroTrading",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("5a127994a9352fdbf6e045f4bfd80884"))
                    }
                });
        }
    }
}
