using Course_service.Models;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

using System.Text.Json.Serialization;
//using Course_service.Controllers;

static bool isRunningInDocker()
{
    return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}

//
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up - Course Service");

try
{
    var builder = WebApplication.CreateBuilder(args);

    //
    builder.Host.UseSerilog((ctx, lc) => lc
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    //2.
    //builder.Services.AddHealthChecks();
    //builder.Services.Configure<HealthCheckPublisherOptions>(options =>
    //{
    //    options.Delay = TimeSpan.FromSeconds(5);
    //    options.Predicate = (check) => check.Tags.Contains("ready");
    //});

    //1.db
    builder.Services.AddDbContext<NetCourseDbContext>(options
        => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    //2.
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            if (isRunningInDocker()) cfg.Host("rabbitmq");

            //cfg.UseDelayedMessageScheduler();

            //var options = new ServiceInstanceOptions()
            //    .SetEndpointNameFormatter(context.GetService<IEndpointNameFormatter>() ?? KebabCaseEndpointNameFormatter.Instance);
            //cfg.ServiceInstance(options, instance =>
            //{
            //    instance.ConfigureJobServiceEndpoints(js =>
            //    {
            //        js.SagaPartitionCount = 1;
            //        js.FinalizeCompleted = true;

            //        js.ConfigureSagaRepositories(context);
            //    });
            //    instance.ConfigureEndpoints(context);
            //});
        });
    });
    //builder.Services.AddOptions<MassTransitHostOptions>().Configure(options =>
    //{
    //    // if specified, waits until the bus is started before returning from IHostedService.StartAsync
    //    options.WaitUntilStarted = true; // default is false
    //    options.StartTimeout = TimeSpan.FromSeconds(10);
    //    options.StopTimeout = TimeSpan.FromSeconds(30);
    //});

    //3.
    builder.Services.AddHttpClient("UsersApi", c =>
    {
        c.BaseAddress = new Uri("http://localhost:5011/");
        if (isRunningInDocker())
        {
            c.BaseAddress = new Uri("http://user-api/");
        }

        c.DefaultRequestHeaders.Add("X-req-sid", "CoursesApi");
    });

    //JsonOptions
    //builder.Services.AddControllers();
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
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

    //app.MapCourseEndpoints();

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