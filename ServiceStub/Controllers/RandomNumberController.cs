using Microsoft.AspNetCore.Mvc;
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
            Console.WriteLine($"Request received: GET /RandomNumber. Mode={ReturnMode}");

            if (ReturnMode == ReturnMode.ok200)
                return Ok(new Random().Next());

            return NotFound();
        }
    }

}