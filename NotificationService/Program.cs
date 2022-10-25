using MassTransit;
using NotificationService.Consumers;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            //1.
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<EnrollCourseConsumer>(typeof(EnrollCourseConsumerDefinition));
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
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

            app.Run();
        }
    }
}