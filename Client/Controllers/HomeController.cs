using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            //call a protected resource on server
            var serverResponse = await AccessTokenRefreshWrapper(() => SecuredGetRequest("https://localhost:44307/secret/index"));
            var serverResponseData = await serverResponse.Content.ReadAsStringAsync();
            ViewBag.ServerMessage = serverResponseData;

            //call a protected resource on api
            var apiResponse = await AccessTokenRefreshWrapper(() => SecuredGetRequest("https://localhost:44314/api/secret"));
            var apiResponseData = await apiResponse.Content.ReadAsStringAsync();
            ViewBag.ApiMessage = apiResponseData;

            return View();
        }

        public async Task<string> RefreshAccessToken()
        {
            //get the refresh token value
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            //prepare client
            var client = _httpClientFactory.CreateClient();

            //set grant_type and refresh_token values
            var requestData = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refreh_token"] = refreshToken
            };

            //set form encoding
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44307/oauth/token")
            {
                Content = new FormUrlEncodedContent(requestData)
            };

            //set authorization header as basic with dummy credentials. In production, it should be real
            var creds = "username:password";
            var bytes = Encoding.UTF8.GetBytes(creds);
            var base64Converted = Convert.ToBase64String(bytes);
            requestMessage.Headers.Add("Authorization", $"BASIC {base64Converted}");

            //get refresh token from server
            var refreshTokenResult = await client.SendAsync(requestMessage);
            var refreshTokenString = await refreshTokenResult.Content.ReadAsStringAsync();
            var refreshTokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(refreshTokenString);

            //extract new tokens
            var newToken = refreshTokenData.GetValueOrDefault("access_token");
            var newRefreshToken = refreshTokenData.GetValueOrDefault("refresh_token");

            //update token in the curent httpcontext
            var authInfo = await HttpContext.AuthenticateAsync("mifco.web");
            authInfo.Properties.UpdateTokenValue("access_token", newToken);
            authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);

            //signin the user with the new tokens
            await HttpContext.SignInAsync("mifco.web", authInfo.Principal, authInfo.Properties);

            return "";
        }

        private async Task<HttpResponseMessage> SecuredGetRequest(string url)
        {
            //get token
            var token = await HttpContext.GetTokenAsync("access_token");

            //create http client
            var client = _httpClientFactory.CreateClient();

            //add authorization header
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            //get and return response 
            return await client.GetAsync(url);
        }

        private async Task<HttpResponseMessage> AccessTokenRefreshWrapper(Func<Task<HttpResponseMessage>> initialRequest)
        {
            //get the response for initial request
            var response = await initialRequest();

            //if response is unauthorized (will have to refresh)
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //refresh the access token
                await RefreshAccessToken();
            }

            //return response
            return response;
        }
    }
}
