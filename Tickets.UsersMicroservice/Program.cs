using Common.Dtos;
using Common.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

var contact = new OpenApiContact()
{
    Name = "TicketManagement",
    Email = "software@iotsl.es",
    Url = new Uri("http://www.iotsl.es")
};

var license = new OpenApiLicense()
{
    Name = "Free License",
    Url = new Uri("http://www.iotsl.es")
};

var info = new OpenApiInfo()
{
    Version = "v1",
    Title = "Minimal API - JWT Authentication with Swagger demo",
    Description = "Implementing JWT Authentication in Minimal API",
    TermsOfService = new Uri("http://www.example.com"),
    Contact = contact,
    License = license
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", info);
    o.AddSecurityDefinition("Bearer", securityScheme);
    o.AddSecurityRequirement(securityReq);
});

var key = Encoding.ASCII.GetBytes("!$Uw6e~T4%tQ@z#sXv9&gYb2^hV*pN7cF");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.Events = new JwtBearerEvents
    {
        OnForbidden = async (context) =>
        {

            context.Response.StatusCode = 403;
            context.HttpContext.Response.ContentType = "application/json";

            var response = new GenericResponseDto();

            response.Error = new GenericErrorDto()
            {
                Id = ResponseCodes.InvalidAccessType,
                Description = "User doesn't have the required access type ",
                Location = "JWT Bearer Middleware"
            };

            // we can write our own custom response content here
            await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
        },
        OnChallenge = async (context) =>
        {

            // this is a default method
            // the response statusCode and headers are set here
            context.HandleResponse();

            context.Response.StatusCode = 401;
            context.HttpContext.Response.ContentType = "application/json";

            var response = new GenericResponseDto();

            // AuthenticateFailure property contains 
            // the details about why the authentication has failed
            if (context.AuthenticateFailure != null)
            {
                response.Error = new GenericErrorDto()
                {
                    Id = ResponseCodes.InvalidToken,
                    Description = "Token Validation Has Failed. Request Access Denied",
                    Location = "JWT Bearer Middleware"
                };

                // we can write our own custom response content here
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
            }
            else
            {

                response.Error = new GenericErrorDto()
                {
                    Id = ResponseCodes.InvalidToken,
                    Description = "Missing token",
                    Location = "JWT Bearer Middleware"
                };

                // we can write our own custom response content here
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
            }
        }
    };
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };
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
    options.User.RequireUniqueEmail = true;
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
    // Apply migrations and make sure that the default users and roles have been created
    dbContext.Database.Migrate();
    var identitiesService = (IdentitiesService)serviceProvider.GetService(typeof(IIdentitiesService));

    
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