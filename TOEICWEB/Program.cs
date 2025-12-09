using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TOEICWEB.Data;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ✅ Thêm DbContext
builder.Services.AddDbContext<SupabaseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Cấu hình JWT Authentication
var defaultKey = "your-secret-key-here-minimum-32-characters-long-for-security";
var jwtKey = builder.Configuration["Jwt:Key"] ?? defaultKey;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "toeic-app";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "toeic-users";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        // Add verbose diagnostics to understand why tokens are rejected (dev only)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[JWT] Authentication failed: {ctx.Exception.GetType().Name} - {ctx.Exception.Message}");
                if (ctx.Request?.Headers?.ContainsKey("Authorization") == true)
                {
                    var header = ctx.Request.Headers["Authorization"].ToString();
                    Console.WriteLine($"[JWT] Authorization header present, length={header.Length}");
                }
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                // Runs when a request is unauthorized
                Console.WriteLine($"[JWT] Challenge triggered. Error={ctx.Error} Desc={ctx.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                // Log the subject/nameid after successful validation
                var nameId = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                Console.WriteLine($"[JWT] Token validated for user: nameid={nameId}, email={email}");
                return Task.CompletedTask;
            }
        };
    });

// ✅ Thêm Authorization
builder.Services.AddAuthorization();

// ✅ Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ✅ Thêm Controllers
builder.Services.AddControllers();

// ✅ Thêm Swagger với JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// ✅ Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chỉ bật chuyển hướng HTTPS trong môi trường triển khai khi chắc chắn có cấu hình HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ✅ Sử dụng CORS (phải trước Authentication)
app.UseCors("AllowAll"); // hoặc "AllowAll"

// 🔎 Ghi log nhanh header Authorization trước khi vào Authentication để xác nhận client có gửi token
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("Authorization", out var auth))
    {
        var raw = auth.ToString();
        var preview = raw.Length > 20 ? raw.Substring(0, 20) + "..." : raw;
        Console.WriteLine($"[HTTP] Authorization header received: {preview}");
    }
    else
    {
        Console.WriteLine("[HTTP] No Authorization header on request " + context.Request.Path);
    }
    await next();
});

// ✅ Sử dụng Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ping endpoint (ẩn danh) để kiểm tra routing và CORS nhanh
app.MapGet("/api/ping", () => Results.Ok(new { message = "pong", time = DateTime.UtcNow }))
    .AllowAnonymous();

app.Run();
