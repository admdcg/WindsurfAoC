using Microsoft.EntityFrameworkCore;
using WindsurfAoC.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar el entorno
var environment = builder.Environment;
builder.Configuration
    .SetBasePath(environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

// Configure base path for production
if (builder.Environment.IsProduction())
{
    builder.Services.Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = true;
    });
}

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WindsurfAoC API", Version = "v1" });

    // Configurar la autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(
                "http://localhost:5173",  // URL de desarrollo
                "http://localhost",       // URL local IIS
                "http://localhost:80",    // URL local IIS con puerto
                "https://localhost",      // HTTPS local
                "https://localhost:443"   // HTTPS local con puerto
                // Añade aquí tu dominio de producción si es diferente
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configurar JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["JwtSettings:Key"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Aplicar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Iniciando migración de base de datos...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Verificar la conexión a la base de datos
        logger.LogInformation("Verificando conexión a la base de datos...");
        if (context.Database.CanConnect())
        {
            logger.LogInformation("Conexión a la base de datos exitosa.");
            
            // Verificar si hay migraciones pendientes
            var pendingMigrations = context.Database.GetPendingMigrations();
            var pendingList = pendingMigrations.ToList();
            if (pendingList.Any())
            {
                logger.LogInformation($"Hay {pendingList.Count} migraciones pendientes. Aplicando...");
                foreach (var migration in pendingList)
                {
                    logger.LogInformation($"Migración pendiente: {migration}");
                }
                context.Database.Migrate();
                logger.LogInformation("Migraciones aplicadas exitosamente.");
            }
            else
            {
                logger.LogInformation("No hay migraciones pendientes.");
            }
        }
        else
        {
            logger.LogError("No se pudo conectar a la base de datos.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la migración de la base de datos: {Message}", ex.Message);
        if (ex.InnerException != null)
        {
            logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UsePathBase("/backend/api");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("../swagger/v1/swagger.json", "WindsurfAoC API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
