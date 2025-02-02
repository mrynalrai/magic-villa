// using Serilog;   // For logging to a log file

using MagicVilla.Villa.Api;
using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Repositories;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Services;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/*
 * For logging to a log file
 
Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
    .WriteTo.File("log/villaLogs.txt",rollingInterval: RollingInterval.Day).CreateLogger();

builder.Host.UseSerilog();
*/
builder.Services.AddDbContext<ApplicationDbContext>(option =>{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSqlConnection"));
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddResponseCaching();
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddApiVersioning(options => {
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");
var audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");
var issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options => {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ClockSkew = TimeSpan.Zero
        };
        // Custom logic to extract token from cookies
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Check if the request contains the "jwt" cookie
                if (context.Request.Cookies.ContainsKey("jwt"))
                {
                    context.Token = context.Request.Cookies["jwt"];
                }
                return Task.CompletedTask;
            }
        };
});

builder.Services.AddControllers(option => {
    option.CacheProfiles.Add("Default30",
        new CacheProfile()
        {
            Duration = 30   // 30 seconds
        }
    );
    // option.ReturnHttpNotAcceptable=true;
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => 
    {
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "MagicVillaV2");
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MagicVillaV1");
    });
}
else
{
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_VillaV1");
        options.RoutePrefix = "";   // the Swagger UI will be served at the root URL of the application 
    });
}

app.UseExceptionHandler("/ErrorHandling/ProcessError");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
