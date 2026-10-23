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
    
    // Seed usuarios si no existen
    if (!context.Usuarios.Any())
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var admin = new SIGEP.Domain.Entities.Usuario(
            username: "jefe_admin",
            passwordHash: passwordHash,
            rolId: 1
        );
        var encargado = new SIGEP.Domain.Entities.Usuario(
            username: "encargado_operativo",
            passwordHash: passwordHash,
            rolId: 2
        );
        context.Usuarios.AddRange(admin, encargado);
        context.SaveChanges();
    }

    // Seed empleados iniciales si no existen
    if (!context.Empleados.Any())
    {
        var fechaBase = DateTime.UtcNow.AddYears(-3);
        var empleadosIniciales = new List<SIGEP.Domain.Entities.Empleado>
        {
            new("Lucía", "Gómez", "32456781", funcionId: 1, regimenId: 1, turnoHabitualId: 1, direccion: "Av. San Martín 450", telefono: "381-4567891", email: "lucia.gomez@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-18)),
            new("Martín", "Díaz", "30127903", funcionId: 2, regimenId: 1, turnoHabitualId: 2, direccion: "Belgrano 120", telefono: "381-4567892", email: "martin.diaz@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-24)),
            new("Carla", "Ruiz", "28440215", funcionId: 3, regimenId: 2, turnoHabitualId: 4, direccion: "9 de Julio 310", telefono: "381-4567893", email: "carla.ruiz@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-30)),
            new("Diego", "López", "27968540", funcionId: 1, regimenId: 2, turnoHabitualId: 5, direccion: "Rivadavia 880", telefono: "381-4567894", email: "diego.lopez@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-36)),
            new("Ana", "Medina", "33024719", funcionId: 4, regimenId: 1, turnoHabitualId: 3, direccion: "Sarmiento 215", telefono: "381-4567895", email: "ana.medina@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-12)),
            new("José", "Pérez", "29813004", funcionId: 1, regimenId: 2, turnoHabitualId: 6, direccion: "Mitre 740", telefono: "381-4567896", email: "jose.perez@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-40)),
            new("Mónica", "Sosa", "31444229", funcionId: 5, regimenId: 1, turnoHabitualId: 1, direccion: "Alberdi 55", telefono: "381-4567897", email: "monica.sosa@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-20)),
            new("Raúl", "Fernández", "26880331", funcionId: 2, regimenId: 2, turnoHabitualId: 5, direccion: "San Lorenzo 1020", telefono: "381-4567898", email: "raul.fernandez@simoca.gob.ar", fechaIngreso: fechaBase.AddMonths(-48))
        };
        context.Empleados.AddRange(empleadosIniciales);
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
