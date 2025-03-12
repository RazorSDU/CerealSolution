using CerealApi.Data;
using CerealApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

//For anyone reading this, good luck! This is a lot of code to take in at once, because of my requirement to have intergration tests while also needing to enforce HTTPS.

var builder = WebApplication.CreateBuilder(args);

// --- DB context setup ---
builder.Services.AddDbContextPool<CerealContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CerealDatabase"),
    sqlOptions =>
{
sqlOptions.EnableRetryOnFailure(5);
}));

// --- Serilog Setup ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
        .WriteTo.File("Logs/trace.log"))
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
        .WriteTo.File("Logs/debug.log"))
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.File("Logs/info.log"))
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
        .WriteTo.File("Logs/warn.log"))
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File("Logs/error.log"))
    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
        .WriteTo.File("Logs/fatal.log"))
    .WriteTo.File("Logs/combined.log")
    .CreateLogger();

builder.Host.UseSerilog();

// --- JWT Auth ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
// Enforce HTTPS for JWT requests
options.RequireHttpsMetadata = true;
options.SaveToken = true;
options.TokenValidationParameters = new TokenValidationParameters
{
ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
ValidateIssuer = true,
ValidIssuer = issuer,
ValidateAudience = true,
ValidAudience = audience,
ValidateLifetime = true
};
});

// --- Enforce HTTPS Globally (Production/Staging) ---
if (!builder.Environment.IsDevelopment())
{
builder.Services.Configure<MvcOptions>(options =>
{
// Forces HTTPS on all controllers unless in dev/test
options.Filters.Add(new RequireHttpsAttribute());
});
}

builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cereal API", Version = "v1" });
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
Name = "Authorization",
Type = SecuritySchemeType.Http,
Scheme = "Bearer",
BearerFormat = "JWT",
In = ParameterLocation.Header,
Description = "Enter 'Bearer {token}'...",
});
c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// --- Rate Limiter (Disable for Tests, but not for RateLimitTest) ---
if (!builder.Environment.IsEnvironment("Test") || builder.Environment.IsEnvironment("RateLimitTest"))
{
builder.Services.AddRateLimiter(options =>
{
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
{
var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
{
PermitLimit = 5,                     // triggers quickly for demonstration/tests
Window = TimeSpan.FromSeconds(10),
QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
QueueLimit = 0
});
});
options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
}

var app = builder.Build();

// -- Migrate & Seed if empty --
using (var scope = app.Services.CreateScope())
{
var db = scope.ServiceProvider.GetRequiredService<CerealContext>();
db.Database.Migrate();

var csvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "Cereal.csv");
var imagesFolderPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "Images");

CsvImporter.ImportCereals(csvPath, imagesFolderPath, db);
}

// Ensure logs folder
Directory.CreateDirectory("Logs");

// --- HTTPS Redirection in Non-Test Environments ---
if (!builder.Environment.IsEnvironment("Test"))
{
app.UseHttpsRedirection();
}

// --- HSTS in Non-Dev Environments (i.e. Production) ---
if (!app.Environment.IsDevelopment())
{
app.UseHsts();
}

// --- Rate Limiter Globally (not in Test) ---
if (!builder.Environment.IsEnvironment("Test"))
{
app.UseRateLimiter();
}

// --- Enable Swagger on HTTPS (Dev or Test) ---
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
app.UseSwagger();
app.UseSwaggerUI(options =>
{
// You can access at: https://localhost:<port>/swagger
options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cereal API v1");
options.RoutePrefix = "swagger";
});
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- Block HTTP in Production, but NOT in RateLimitTest ---
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("RateLimitTest"))
{
app.Use(async (context, next) =>
{
// If the request is not HTTPS => 403 Forbidden
if (!context.Request.IsHttps)
{
context.Response.StatusCode = StatusCodes.Status403Forbidden;
await context.Response.WriteAsync("HTTPS is required.");
return;
}
await next();
});
}

app.Run();

// Make Program accessible for integration tests
public partial class Program { }