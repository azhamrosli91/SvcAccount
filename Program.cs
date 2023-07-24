using libMasterLibaryApi.ContextRepo;
using libMasterLibaryApi.Helpers;
using libMasterLibaryApi.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SvcAccount.DBContext;
using SvcAccount.Interface;
using SvcAccount.Model;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddCors();
builder.Services.AddScoped<IWebApiCalling, WebApiCalling>();
builder.Services.AddScoped<IImageConvert, ImageConverter>();
builder.Services.AddScoped<IJwtToken, JWTToken>();
builder.Services.AddScoped<IApiURL, ApiURLRepo>();
builder.Services.AddScoped<IDbService, DBContext>();

var connectionString = builder.Configuration.GetConnectionString("Companydb");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => builder
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .SetIsOriginAllowed((host) => true)
                 .AllowCredentials()
                 .WithOrigins("https://localhost")
             //.WithOrigins("https://localhost:32777") //SvcAccount   Docker
             //.WithOrigins("https://localhost:32779") //SvcEmail     Docker
             //.WithOrigins("https://localhost:32781") //EpsilonSigma Docker
             //.WithOrigins("https://localhost:44358") //SvcAccount   IIS
             //.WithOrigins("https://localhost:44301") //SvcEmail     IIS
             //.WithOrigins("https://localhost:44326") //EpsilonSigma IIS
             );

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
