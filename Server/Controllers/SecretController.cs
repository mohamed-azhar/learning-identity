using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    public class SecretController : Controller
    {
        [Authorize]
        public string Index() => "secret message which is protected by the server";

    }
}
