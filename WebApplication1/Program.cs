


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1.Models;
using Microsoft.OpenApi.Models;
using WebApplication1.Filters;
using System.Reflection;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// JWT ayarlarýný yapýlandýrma
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// JWT doðrulama ve oluþturma servislerini ekleme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// Diðer servisleri ekleme
builder.Services.Configure<ConfigurationSettings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add JWT authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT token into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    // Add file upload support
    c.OperationFilter<SwaggerFileOperationFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
        builder =>
        {
            builder.WithOrigins("http://192.168.1.35:3000", "http://localhost:3000");
        });
});

// Forex API servisini ekleme
builder.Services.AddHttpClient();

//appsettingsten okuma
var configurationSettings = new ConfigurationSettings();
builder.Configuration.GetSection("ConfigurationSettings").Bind(configurationSettings);
builder.Services.AddSingleton(configurationSettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(
    options => options.WithOrigins("*").AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()
);
app.UseHttpsRedirection();

app.UseRouting(); // Ensure UseRouting is called before UseAuthentication and UseAuthorization

app.UseAuthentication(); // JWT kimlik doðrulama middleware'i ekleme
app.UseAuthorization(); // Ensure UseAuthorization is called after UseAuthentication

app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
