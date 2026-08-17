using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Plandi.Library.Models;
using Plandi.Services;
using Plandi.Services.Interfaces;
using Plandi.Services.ProgramaAsignaturaExtraction;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Plandi.Dto.Common;
using Plandi.API.Security;
using Plandi.API.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ?? builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("CAMBIAR_", StringComparison.Ordinal) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configure una Jwt:Key de al menos 32 bytes mediante User Secrets o la variable JWT_SIGNING_KEY.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("RequireDirectorRole", policy =>
        policy.RequireRole("Director"));
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("loginPolicy", context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress ?? IPAddress.None;
        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<PasswordRecoveryRateLimiter>();

/* CORS */
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<List<string>>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        if (allowedOrigins != null)
        {
            policy
                .WithOrigins(allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAutorizacionService, AutorizacionService>();
builder.Services.AddScoped<IGestionRolesUsuarioService, GestionRolesUsuarioService>();
builder.Services.AddScoped<IGestionDocentesPlantillaService, GestionDocentesPlantillaService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAsignaturaService, AsignaturaService>();
builder.Services.AddScoped<ICicloEscolarService, CicloEscolarService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IRelojAcademico, RelojAcademico>();
builder.Services.AddScoped<IPeriodoLifecycleService, PeriodoLifecycleService>();
builder.Services.AddHostedService<PeriodoClosingHostedService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();
builder.Services.AddScoped<IAcademiaService, AcademiaService>();
builder.Services.AddScoped<ICargaAcademicaService, CargaAcademicaService>();
builder.Services.AddScoped<IAdministracionAcademicaService, AdministracionAcademicaService>();
builder.Services.AddScoped<IRepositorioService, RepositorioService>();
builder.Services.AddScoped<IImportacionCargaAcademicaService, ImportacionCargaAcademicaService>();
builder.Services.AddScoped<IProgramaAsignaturaImportService, ProgramaAsignaturaImportService>();
builder.Services.AddScoped<IPlaneacionTemplateService, PlaneacionTemplateService>();
builder.Services.AddScoped<IPlaneacionDocumentosService, PlaneacionDocumentosService>();
builder.Services.AddScoped<IPlaneacionPdfService, PlaneacionPdfService>();
builder.Services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
builder.Services.AddSingleton<ProgramGeneralInfoExtractor>();
builder.Services.AddSingleton<UnitExtractor>();
builder.Services.AddSingleton<TopicTableExtractor>();
builder.Services.AddSingleton<EvaluationTableExtractor>();
builder.Services.AddSingleton<ReferencesExtractor>();
builder.Services.AddSingleton<ProgramaAsignaturaExtractionValidator>();
builder.Services.AddSingleton<ProgramaAsignaturaExtractor>();
builder.Services.AddScoped<IGeneracionPlaneacionesService, GeneracionPlaneacionesService>();
builder.Services.AddScoped<IPlaneacionCaratulaService, PlaneacionCaratulaService>();
builder.Services.AddScoped<IPlaneacionTemaService, PlaneacionTemaService>();
builder.Services.AddScoped<IPlaneacionEvaluacionService, PlaneacionEvaluacionService>();
builder.Services.AddScoped<IPlaneacionSecuenciaService, PlaneacionSecuenciaService>();
builder.Services.AddScoped<IPlaneacionReferenciaService, PlaneacionReferenciaService>();
builder.Services.AddScoped<IMisPlaneacionesDocenteService, MisPlaneacionesDocenteService>();
builder.Services.AddScoped<IEdicionPlaneacionService, EdicionPlaneacionService>();
builder.Services.AddScoped<IAsignacionRevisorPlaneacionService, AsignacionRevisorPlaneacionService>();
builder.Services.AddScoped<IPlaneacionesRevisorService, PlaneacionesRevisorService>();
builder.Services.AddScoped<IEstadoPlaneacionService, EstadoPlaneacionService>();
builder.Services.AddScoped<IComentariosCorreccionService, ComentariosCorreccionService>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<PlaneacionesProfile>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue únicamente el access token obtenido en /api/Auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.Use(async (httpContext, next) =>
{
    try
    {
        await next();
    }
    catch (UnauthorizedAccessException exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(exception.Message));
    }
    catch (NotFoundException exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(exception.Message));
    }
    catch (ForbiddenException exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(exception.Message));
    }
    catch (ConflictException exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(exception.Message));
    }
    catch (AppException exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(exception.Message));
    }
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
