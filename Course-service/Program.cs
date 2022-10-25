using Course_service.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
//using Course_service.Controllers;

var builder = WebApplication.CreateBuilder(args);

//2.
builder.Services.AddMassTransit(x => {
    x.UsingRabbitMq();
});
//builder.Services.AddOptions<MassTransitHostOptions>().Configure(options => {
//    // if specified, waits until the bus is started before returning from IHostedService.StartAsync
//    options.WaitUntilStarted = true; // default is false
//    options.StartTimeout = TimeSpan.FromSeconds(10);
//    options.StopTimeout = TimeSpan.FromSeconds(30);
//});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//1.db
builder.Services.AddDbContext<NetCourseDbContext>(options
    => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
