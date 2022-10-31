using MassTransit;
using NotificationService.Configuration;
using NotificationService.Consumers;
using NotificationService.Services;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Starting up - Notification Service");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                //
                builder.Host.UseSerilog((ctx, lc) => lc
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} <{ProcessId}><{ThreadId}><{ThreadName}>{NewLine}{Exception}"));

                //1.
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

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                //2.
                builder.Services.Configure<Smtp>(builder.Configuration.GetSection("Smtp"));
                builder.Services.AddTransient<IMailService, MailService>();

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