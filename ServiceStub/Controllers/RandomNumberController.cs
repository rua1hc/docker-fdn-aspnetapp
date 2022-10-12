using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using static ServiceStub.Program;

namespace ServiceStub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RandomNumberController : ControllerBase
    {
        public static ReturnMode ReturnMode { get; set; } = ReturnMode.ok200;

        [HttpGet()]
        public ActionResult<string> Get()
        {
            Log.Information("Request received: GET /RandomNumber Mode={ReturnMode}", ReturnMode);

            foreach (var header in Request.Headers)
            {
                Log.Information("{headerKey}={headerValue}", header.Key, header.Value);
            }
            Response.Headers.Add("X-Response-ID", "service-2");

            if (ReturnMode == ReturnMode.ok200)
                return Ok(new Random().Next());

            return NotFound();
        }
    }

}