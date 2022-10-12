using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

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
            Log.Information("Request received: GET /HealthStatus Status={Status}", Status);
            return Status.ToString();
        }

        [HttpPost("SetResponse/{status}")]
        public ActionResult SetResponse(HealthStatus status)
        {
            Log.Information("Request received: POST /HealthStatus");
            Status = status;
            return Ok($"Changed status to {status}");
        }
    }
}
