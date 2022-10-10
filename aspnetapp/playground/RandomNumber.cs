using Polly;
using Polly.CircuitBreaker;
using System.Net;
//using Serilog;

namespace playground.RandomNumber
{
    public class RandomNumber
    {
        private readonly HttpClient HttpClient;
        private readonly string GetRandomNumberUrl;
        private readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy;

        public RandomNumber(string url)
        {
            GetRandomNumberUrl = $"{url}/RandomNumber/";
            HttpClient = new HttpClient();

            CircuitBreakerPolicy = Policy
                .Handle<HttpRequestException>(httpEx => httpEx.StatusCode == HttpStatusCode.NotFound)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: (_, duration) => Log($"Circuit tripped. Circuit is open and requests won't be allowed through for duration={duration}"),
                    onReset: () => Log("Circuit closed. Requests are now allowed through"),
                    onHalfOpen: () => Log("Circuit is now half-opened and will test the service with the next request"));
        }

        public async Task<string> GetRandomNumber()
        {
            try
            {
                return await CircuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var response = await HttpClient.GetAsync(GetRandomNumberUrl);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                });

            }
            catch (HttpRequestException httpEx)
            {
                Log($"Request failed. StatusCode={httpEx.StatusCode} Message={httpEx.Message}");
                return "Failed";
            }
            catch (BrokenCircuitException ex)
            {
                Log($"Request failed due to opened circuit: {ex.Message}");
                return "CircuitBroke";
            }
        }

        private void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now:hh:mm:ss.fffff}\t{message}");
        }
    }


}