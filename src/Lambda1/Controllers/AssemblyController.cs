using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Lambda1.Controllers
{
    [Route("api/[controller]")]
    public class AssemblyController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return typeof(AssemblyController).GetTypeInfo().Assembly.FullName;
        }
    }
}
