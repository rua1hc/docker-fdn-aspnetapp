using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStub.Controllers;
using System;
using System.Threading.Tasks;

namespace ServiceStub
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			string url = "https://localhost:12345";

			var commandLoopTask = Task.Run(() => CommandLoop(url));

			var builder = Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseKestrel()
					.UseStartup<Startup>()
					.UseUrls(url)
					.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
				});


			await Task.WhenAny(builder.RunConsoleAsync(), commandLoopTask);
		}
		private static void CommandLoop(string url)
		{
			Console.WriteLine($"Stubbed endpoint: GET {url}/status");
			Console.WriteLine("Commands:");
			Console.WriteLine("\tset-status <Healthy, Unhealthy, or Degraded> Example: set-status Healthy");

			while (true)
			{
				Console.WriteLine($"Current status: {HealthStatusController.Status}");
				var args = Console.ReadLine().Split();

				if (args.Length < 2 || args[0] != "set-status")
				{
					Console.WriteLine("Invalid command");
					continue;
				}

				if (!Enum.TryParse<HealthStatus>(args[1], ignoreCase: true, out HealthStatus status))
				{
					Console.WriteLine("Invalid value for HealthStatus");
					continue;
				}

				HealthStatusController.Status = status;
				RandomNumberController.ReturnMode = status == HealthStatus.Healthy ? RN_RET_MODE.RET_200_OK : RN_RET_MODE.RET_404_NOTFOUND;
			}
		}
	}
}
