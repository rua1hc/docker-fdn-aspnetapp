using MassTransit;
using NotificationService.Configuration;
using NotificationService.Consumers;
using NotificationService.Services;

using Serilog;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                //.Enrich.WithThreadId()
                //.Enrich.WithThreadName()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

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
            //app.UseSerilogRequestLogging();

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
    }
}