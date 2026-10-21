using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SIGEP.Application.Interfaces;
using SIGEP.Application.Services;
using SIGEP.Data;
using SIGEP.Data.Repositories;
using SIGEP.Domain.Repositories;
using SIGEP.Domain.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Base de Datos PostgreSQL con Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SIGEPDbContext>(options =>
{
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("SIGEP.Data"));
});
builder.Services.AddScoped<SIGEP.Domain.Common.IUnitOfWork>(sp => sp.GetRequiredService<SIGEPDbContext>());

// 2. Registro de Inyección de Dependencias (DI)
// Repositorios (Data -> Domain)
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IFuncionRepository, FuncionRepository>();
builder.Services.AddScoped<IRegimenRepository, RegimenRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
builder.Services.AddScoped<IPlanificacionSabadoRepository, PlanificacionSabadoRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();

// Servicios de Dominio (Domain)
builder.Services.AddScoped<ISabadoRotacionDomainService, SabadoRotacionDomainService>();
builder.Services.AddScoped<IEventScoringDomainService, EventScoringDomainService>();

// Servicios de Aplicación (Application)
builder.Services.AddScoped<IAuthAppService, AuthAppService>();
builder.Services.AddScoped<IPersonalAppService, PersonalAppService>();
builder.Services.AddScoped<IAsistenciaAppService, AsistenciaAppService>();
builder.Services.AddScoped<IPlanificacionSabadoAppService, PlanificacionSabadoAppService>();
builder.Services.AddScoped<IEventoAppService, EventoAppService>();

// 3. Configuración de Autenticación con JWT
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super_secreto_sigep_simoca_2026_jwt_key_minimo_32_caracteres!";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Configuración de CORS para el Frontend de React (Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// 5. Seeding automático de datos iniciales
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SIGEPDbContext>();
    if (!context.Usuarios.Any())
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var admin = new SIGEP.Domain.Entities.Usuario(
            username: "jefe_admin",
            passwordHash: passwordHash,
            rolId: 1
        );
        context.Usuarios.Add(admin);
        context.SaveChanges();
    }
}

// 6. Pipeline HTTP
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "OK",
    framework = ".NET 8.0 / C#",
    architecture = "4-Capas (Domain, Data, Application, Api)",
    timestamp = DateTime.UtcNow
}));

app.Run();
