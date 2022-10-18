using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApp1.Config;
using WebApp1.Data;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));


//builder.Services.AddDefaultIdentity<IdentityUser>(
//    opt =>
//    {
//        opt.SignIn.RequireConfirmedAccount = false;

//        opt.Password.RequireDigit = false;
//        opt.Password.RequireLowercase = true;
//        opt.Password.RequireUppercase = false;
//        opt.Password.RequireNonAlphanumeric = false;
//        opt.Password.RequiredLength = 5;
//        opt.Password.RequiredUniqueChars = 0;
//    })
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddDefaultIdentity<ApplicationUser>(
//    opt =>
//        opt.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//1.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    opt =>
    {
        opt.SignIn.RequireConfirmedAccount = false;

        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = true;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequiredLength = 5;
        opt.Password.RequiredUniqueChars = 0;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

//2.
builder.Services.ConfigureApplicationCookie(
     opt =>
     {
         opt.Cookie.HttpOnly = true;
         //options.Cookie.Expiration 

         opt.ExpireTimeSpan = TimeSpan.FromMinutes(15);
         opt.LoginPath = "/Identity/Account/Login";
         opt.LogoutPath = "/Identity/Account/Logout";
         opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
         opt.SlidingExpiration = true;
         //options.ReturnUrlParameter=""
     });

//JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = false,
};
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(
    opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);

        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });

//3.
//builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    //Inspect the context.user.identies object 
    await next.Invoke();
});

app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();


