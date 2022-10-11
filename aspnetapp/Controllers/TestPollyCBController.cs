using Microsoft.AspNetCore.Mvc;
using Polly;

namespace aspnetapp.Controllers;

[ApiController]
[Route("[controller]")]
public class TestPollyCBController : ControllerBase
{
    private readonly ILogger<TestPollyCBController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    public TestPollyCBController(ILogger<TestPollyCBController> logger, IHttpClientFactory httpClientFactory, IAsyncPolicy<HttpResponseMessage> policy)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _policy = policy;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
        var client = _httpClientFactory.CreateClient("randApi");
        var response = await _policy.ExecuteAsync(() => client.GetAsync("randomnumber"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

}
