using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace ServiceStub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthStatusController : ControllerBase
    {
        public static HealthStatus Status { get; set; } = HealthStatus.Healthy;
        [HttpGet()]
        public string Get()
        {
            Console.WriteLine("Request received: GET /HealthStatus");
            return Status.ToString();
        }

        [HttpPost("SetResponse/{status}")]
        public ActionResult SetResponse(HealthStatus status)
        {
            Console.WriteLine("Request received: POST /HealthStatus");
            Status = status;
            return Ok($"Changed status to {status}");
        }
    }
}
