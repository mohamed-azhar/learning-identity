using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _client;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var protectedServerResourceRequest = await _client.GetAsync("https://localhost:44307/secret/index");
            var serverResponse = await protectedServerResourceRequest.Content.ReadAsStringAsync();
            ViewBag.ServerMessage = serverResponse;

            var protectedApiResourceRequest = await _client.GetAsync("https://localhost:44314/api/secret");
            var apiResponse = await protectedApiResourceRequest.Content.ReadAsStringAsync();
            ViewBag.ApiMessage = apiResponse;

            return View();
        }
    }
}
