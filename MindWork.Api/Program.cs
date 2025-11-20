using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;
using MindWork.Api.Data;
using MindWork.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

// =============================
// CONFIG JWT (appsettings.json ou fallback)
// =============================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-mindwork-key-123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MindWork.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MindWork.Client";

// =============================
// CONFIGURAÇÃO DO EF CORE
// =============================
builder.Services.AddDbContext<MindWorkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================
// CONFIGURAÇÃO DE CONTROLLERS + JSON (ENUMS COMO STRING)
// =============================
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // enums como string no JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// =============================
// AUTENTICAÇÃO JWT
// =============================
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// =============================
// SWAGGER (modo simples, sem OpenApi.Models)
// =============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =============================
// VERSIONAMENTO DA API
// =============================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// =============================
// CORS
// =============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// =============================
// GARANTIR CRIAÇÃO DO BANCO DE DADOS + SEED ADMIN
// =============================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MindWorkDbContext>();
    db.Database.EnsureCreated();

    // 1) Garante uma organização padrão
    var org = db.Organizations.FirstOrDefault(o => o.Name == "MindWork HQ");
    if (org == null)
    {
        org = new Organization
        {
            Name = "MindWork HQ"
        };

        db.Organizations.Add(org);
        db.SaveChanges();
    }

    // 2) Garante um usuário admin padrão
    var admin = db.Users.FirstOrDefault(u => u.Email == "admin@mindwork.com");
    if (admin == null)
    {
        admin = new User
        {
            Name = "Admin Global",
            Email = "admin@mindwork.com",
            Password = "Admin123!",
            Role = "Admin",
            OrganizationId = org.Id
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// =============================
// PIPELINE
// =============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MindWork API v1");
        options.RoutePrefix = "swagger"; // URL: /swagger
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MindWork API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
