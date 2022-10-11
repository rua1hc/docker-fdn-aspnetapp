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
			Console.WriteLine("\tset-status <ok200, nf404> Example: set-status ok200");

			while (true)
			{
				Console.WriteLine($"Current return mode: {RandomNumberController.ReturnMode}");
				var args = Console.ReadLine().Split();

				if (args.Length < 2 || args[0] != "set-status")
				{
					Console.WriteLine("Invalid command");
					continue;
				}

				if (!Enum.TryParse<ReturnMode>(args[1], ignoreCase: true, out ReturnMode status))
				{
					Console.WriteLine("Invalid status value");
					continue;
				}

				RandomNumberController.ReturnMode = status;
				HealthStatusController.Status = status == ReturnMode.ok200 ? HealthStatus.Healthy : HealthStatus.Unhealthy;
			}
		}

		public enum ReturnMode
		{
			ok200,
			nf404
		}
	}
}
