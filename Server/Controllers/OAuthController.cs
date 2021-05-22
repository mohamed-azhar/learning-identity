using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class OAuthController : Controller
    {
        [HttpGet]
        public IActionResult Authorize(string response_type, string client_id, string redirect_uri, string scope, string state)
        {
            var queryBuilder = new QueryBuilder();
            queryBuilder.Add("redirectUri", redirect_uri);
            queryBuilder.Add("state", state);
            return View(model: queryBuilder.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(string username, string redirectUri, string state)
        {
            string code = "yoooooo";

            var queryBuilder = new QueryBuilder();
            queryBuilder.Add("code", code);
            queryBuilder.Add("state", state);

            return Redirect($"{redirectUri}{queryBuilder}");
        }

        public async Task<IActionResult> Token(string grant_type, string code, string redirect_uri, string client_id, string refresh_token)
        {
            //prepare claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.GivenName, "john"),
                new Claim(JwtRegisteredClaimNames.Email, "john@email.com"),
                new Claim("custom_claim", "hello"),
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
                grant_type == "refresh_token" ? DateTime.Now.AddMinutes(5) : DateTime.Now.AddMilliseconds(1),
                signingCredentials);

            //generate jwt using handler
            var access_token = new JwtSecurityTokenHandler().WriteToken(token);

            var responseObject = new
            {
                access_token,
                token_type = "Bearer",
                custom_claim = "my custom claim haha",
                refresh_token = "this-is-a-sample-rfresh-token"
            };

            var response = System.Text.Json.JsonSerializer.Serialize(responseObject);
            var bytes = Encoding.UTF8.GetBytes(response);

            await Response.Body.WriteAsync(bytes, 0, bytes.Length);

            return Redirect(redirect_uri);
        }

        [Authorize]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Query.TryGetValue("access_token", out var token))
            {
                return Ok();
            }
            return BadRequest();
        }
    }
}
