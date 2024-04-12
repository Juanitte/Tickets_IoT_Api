using Common.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Authorization;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddOcelot();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("!$Uw6e~T4%tQ@z#sXv9&gYb2^hV*pN7cF"))
        };
    });
// Add services to the container.

var app = builder.Build();
app.UseCors("MyPolicy");

var configuration = new OcelotPipelineConfiguration
{
    PreAuthenticationMiddleware = async (ctx, next) =>
    {
        try
        {
            var authoritation = ctx.Request.Headers.FirstOrDefault(f => f.Key.Equals("Authorization"));
            if (authoritation.Value.Any())
            {
                var encToken = authoritation.Value.FirstOrDefault();
                if (encToken.Contains("Bearer "))
                    encToken = encToken.Replace("Bearer ", "");
                var token = IoTEncoder.DecodeString(encToken);

                ctx.Request.Headers.Remove("Authorization");
                ctx.Request.Headers.Add("Authorization", "Bearer " + token);
                var request = ctx.Items.DownstreamRequest();
                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", token);
            }
        }
        catch (Exception ex)
        {
            ctx.Items.SetError(new UnauthorizedError("your custom message"));
            return;
        }

        await next.Invoke();
    }
};

app.UseOcelot(configuration).Wait();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();
app.Run();