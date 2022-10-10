using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up -");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    //1.
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:5341"));
    // .ReadFrom.Configuration(ctx.Configuration));
    //2.DI
    //builder.Services.AddTransient<IOperationTransient, Operation>();
    //builder.Services.AddScoped<IOperationScoped, Operation>();
    //builder.Services.AddSingleton<IOperationSingleton, Operation>();

    //3.
    // Add services to the container.
    builder.Services.AddDbContext<DotNetTraining>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });

    var app = builder.Build();

    //1.
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    //2.DI
    //app.MiddlewareExtented();
    //app.MiddlewareExtentedMore();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

    //1.
    app.MapGet("/env", () =>
    {
        return new EnvironmentInfo();
    });

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
