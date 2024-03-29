using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RequestFiltering.Services;
using Serilog;
using System.Globalization;
using System.Text.Json.Serialization;
using Tickets.TicketsMicroservice.Models.Context;
using Tickets.TicketsMicroservice.Models.UnitsOfWork;
using Tickets.TicketsMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ILoggerFactory loggerFactory = new LoggerFactory();

loggerFactory.AddSerilog(new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.RollingFile("C:/Datos/GET/TrackssMicroservice/log-{Date}.txt")
                            .CreateLogger());

builder.Services.AddSingleton(typeof(ILoggerFactory), loggerFactory);
builder.Services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger), loggerFactory.CreateLogger("EatEazy_RestaurantsMicroservice"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<TicketsDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IoTUnitOfWork>();

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

//A�adimos servicio de filtro
builder.Services.AddTransient<IBlockingService, BlockingService>();
builder.Services.AddScoped<ITicketsService, TicketsService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<IAttachmentsService, AttachmentsService>();


//A�adimos los servicios necesarios

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

app.UseCors("MyPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    var context = app.Services.GetRequiredService<TicketsDbContext>();
    context.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
