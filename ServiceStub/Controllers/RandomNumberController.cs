using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("[controller]")]
public class RandomNumberController : ControllerBase
{
    public static RN_RET_MODE ReturnMode { get; set; } = RN_RET_MODE.RET_200_OK;

    [HttpGet()]
    public ActionResult<string> Get()
    {
        Console.WriteLine($"Request received: GET /RandomNumber. Mode={ReturnMode}");

        if (ReturnMode == RN_RET_MODE.RET_200_OK)
            return Ok(new Random().Next());

        return NotFound();
    }
}

public enum RN_RET_MODE
{
    RET_200_OK,
    RET_404_NOTFOUND
}