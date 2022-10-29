using Course_service.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
//using Course_service.Controllers;

static bool isRunningInDocker() {
    return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}

var builder = WebApplication.CreateBuilder(args);

//1.db
builder.Services.AddDbContext<NetCourseDbContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//2.
builder.Services.AddMassTransit(x => {
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
//builder.Services.AddOptions<MassTransitHostOptions>().Configure(options => {
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
