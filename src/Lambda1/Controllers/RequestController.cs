using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;

namespace Lambda1.Controllers
{
    [Route("api/[controller]")]
    public class RequestController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var proxyRequest = ControllerContext.HttpContext.Items["APIGatewayRequest"] as APIGatewayProxyRequest;
            var lambdaContext = ControllerContext.HttpContext.Items["LambdaContext"] as ILambdaContext;

            return Ok(new Dictionary<string, object>
            {
                { nameof(Request.Method), Request.Method },
                { nameof(Request.Scheme), Request.Scheme },
                { nameof(Request.Host), Request.Host },
                { nameof(Request.Path), Request.Path },
                { nameof(Request.PathBase), Request.PathBase },
                { nameof(Request.Query), Request.Query },
                { nameof(Request.Headers), Request.Headers },
                { "APIGatewayRequest", proxyRequest },
                { "LambdaContext", lambdaContext },
                { "proxy", proxyRequest?.PathParameters["proxy"] },
                { nameof(ControllerContext.HttpContext.Items), string.Join(";", ControllerContext.HttpContext.Items.Keys) }
            });
        }
    }
}