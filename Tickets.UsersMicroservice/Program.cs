using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RequestFiltering.Services;
using Serilog;
using System.Globalization;
using System.Text;
using Tickets.UsersMicroservice.Models.Context;
using Tickets.UsersMicroservice.Models.Entities;
using Tickets.UsersMicroservice.Models.UnitsOfWork;
using Tickets.UsersMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Remove security configuration from Swagger
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

var key = Encoding.ASCII.GetBytes("!$Uw6e~T4%tQ@z#sXv9&gYb2^hV*pN7cF");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    // Your JWT Bearer configuration...
});

builder.Services.AddAuthorization();

#region Log

ILoggerFactory loggerFactory = new LoggerFactory();

loggerFactory.AddSerilog(new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.RollingFile("C:/ProyectoIoT/Back/Logs/log-{Date}.txt")
                            .CreateLogger());

builder.Services.AddSingleton(typeof(ILoggerFactory), loggerFactory);
builder.Services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger), loggerFactory.CreateLogger("UserMicroservices"));

#endregion

#region Base de datos

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IoTUnitOfWork>();

#endregion

#region Identity

builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<UsersDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole<int>>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.AllowedForNewUsers = false;
});

#endregion

#region Traducciones

builder.Services.AddLocalization(options => options.ResourcesPath = "Translations");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
                    new CultureInfo("en-US"),
                    new CultureInfo("es-ES")
                };

    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>
    {
        return await Task.FromResult(new ProviderCultureResult("en"));
    }));
});

#endregion

#region Servicios

builder.Services.AddTransient<IBlockingService, BlockingService>();
builder.Services.AddScoped<IIdentitiesService, IdentitiesService>();
builder.Services.AddScoped<IUsersService, UsersService>();

#endregion

builder.Services.AddAuthorization();

var app = builder.Build();

//app.UseMiddleware<RequestMiddleware>();

app.UseCors("MyPolicy");

#region Migration

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var dbContext = serviceProvider.GetRequiredService<UsersDbContext>();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var identitiesService = (IdentitiesService)serviceProvider.GetService(typeof(IIdentitiesService));

    // Apply migrations and make sure that the default users and roles have been created
    dbContext.Database.Migrate();
}

#endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    });
}

app.MapControllers();
app.Run();