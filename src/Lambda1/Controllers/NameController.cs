using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    public class NameController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return Assembly.GetEntryAssembly().FullName;
        }
    }
}
