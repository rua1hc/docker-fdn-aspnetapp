using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up - aspnetapp");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllersWithViews();

    //1.
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    //2.DI
    //builder.Services.AddTransient<IOperationTransient, Operation>();
    //builder.Services.AddScoped<IOperationScoped, Operation>();
    //builder.Services.AddSingleton<IOperationSingleton, Operation>();

    //3.
    builder.Services.AddDbContext<NetUserDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString);
        //options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });

    //4.
    builder.Services.AddSwaggerGen();
    //builder.Services.AddSwaggerDocument();

    //5.
    var httpRetryPolicy = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(10),
                onBreak: (_, duration) => Log.Warning($"Circuit tripped. Circuit is open and requests won't be allowed through for duration={duration}"),
                onReset: () => Log.Warning("Circuit closed. Requests are now allowed through"),
                onHalfOpen: () => Log.Warning("Circuit is now half-opened and will test the service with the next request"));
    //.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

    builder.Services.AddHttpClient("randApi", c =>
    {
        c.BaseAddress = new Uri("https://localhost:12345/");
        c.DefaultRequestHeaders.Add("X-token", "add-header-1");
        c.DefaultRequestHeaders.Add("X-REQUEST-ID", "service-1");
    });
    builder.Services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(httpRetryPolicy);

    var app = builder.Build();

    //1.
    app.UseSerilogRequestLogging();

    //4.
    app.UseSwagger();
    app.UseSwaggerUI();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        //app.UseOpenApi();
        //app.UseSwaggerUi3();

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
