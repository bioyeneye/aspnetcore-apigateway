using Microsoft.AspNetCore.Mvc;

namespace AuxApiGateway.Controllers
{
    [ApiController]
    [Route("/v1/api/[controller]")]
    public class AuthenticationController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}