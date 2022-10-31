using MassTransit;

using Microsoft.EntityFrameworkCore;

using ReportService.Consumers;
using ReportService.Models;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace ReportService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Starting up - Report Service");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                //
                builder.Host.UseSerilog((ctx, lc) => lc
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console());

                //
                builder.Services.AddMassTransit(x =>
                {
                    x.AddConsumer<EnrollCourseConsumer>(typeof(EnrollCourseConsumerDefinition));
                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                            cfg.Host("rabbitmq");

                        cfg.ConfigureEndpoints(context);
                    });
                });

                //1.db
                builder.Services.AddDbContext<NetReportDbContext>(options
                    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                //
                app.UseSerilogRequestLogging(options =>
                {
                    options.MessageTemplate = "Handled {RequestPath}";

                    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    };
                });

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception");
            }
            finally
            {
                Log.Information("Shut down complete");
                Log.CloseAndFlush();
            }
        }
    }
}