using Microsoft.AspNetCore.Mvc;
using Polly;

namespace RandomNumberApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RandomNumberController : ControllerBase
    {
        private readonly ILogger<RandomNumberController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public RandomNumberController(ILogger<RandomNumberController> logger, IHttpClientFactory httpClientFactory, IAsyncPolicy<HttpResponseMessage> policy)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _policy = policy;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var client = _httpClientFactory.CreateClient("randomnumber");
            var response = await _policy.ExecuteAsync(() => client.GetAsync("randomnumber"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }
}
