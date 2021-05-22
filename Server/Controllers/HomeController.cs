using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        public IActionResult Authenticate()
        {
            //prepare claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.GivenName, "john"),
                new Claim(JwtRegisteredClaimNames.Email, "john@email.com"),
                new Claim("Custom claim", "69"),
            };

            //prepare identity and principle based on claims
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);

            //generate byte[] for secret 
            var secretbytes = Encoding.UTF8.GetBytes(Constants.Secret);

            //prepare key, algorithm and signing credentials
            var key = new SymmetricSecurityKey(secretbytes);
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(key, algorithm);

            //prepare jwt object
            var token = new JwtSecurityToken(
                Constants.Issuer,
                Constants.Audience,
                claims,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                signingCredentials);

            //generate jwt using handler
            var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { access_token = tokenJson });
        }

        public IActionResult Decode(string part)
        {
            var bytes = Convert.FromBase64String(part);
            var original = Encoding.UTF8.GetString(bytes);
            return Ok(original);
        }
    }
}
