using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ServiceStub.Controllers;
using System;
using System.Threading.Tasks;

namespace ServiceStub
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.ReadFrom.Configuration(configuration)
				.CreateLogger();

			Log.Information("Starting up - ServiceStub");

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
			Log.Information("Stubbed endpoint: GET {url}/status", url);
			Log.Information("Commands: set-status [ok200, nf404]");

			while (true)
			{
				Log.Information("Current return mode: {ReturnMode}", RandomNumberController.ReturnMode);
				var args = Console.ReadLine().Split();

				if (args.Length < 2 || args[0] != "set-status")
				{
					Log.Information("Invalid command");
					continue;
				}

				if (!Enum.TryParse<GeneralReturnMode>(args[1], ignoreCase: true, out GeneralReturnMode status))
				{
					Log.Information("Invalid status value");
					continue;
				}

				RandomNumberController.ReturnMode = status;
				HealthStatusController.Status = status == GeneralReturnMode.ok200 ? HealthStatus.Healthy : HealthStatus.Unhealthy;
			}
		}

		public enum GeneralReturnMode
		{
			ok200,
			nf404
		}
	}
}
