using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecurityPRO.ERP.Web.Middleware;
using SecurityPRO.ERP.Web.Repositories;
using SecurityPRO.ERP.Web.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Repositorios
builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<KardexRepository>();
builder.Services.AddScoped<CotizacionRepository>();
builder.Services.AddScoped<PedidoRepository>();
builder.Services.AddScoped<ServicioTecnicoRepository>();
builder.Services.AddScoped<FacturaRepository>();
builder.Services.AddScoped<CuentaCobrarRepository>();
builder.Services.AddScoped<CuentaPagarRepository>();
builder.Services.AddScoped<SsomaRepository>();
builder.Services.AddScoped<ComisionRepository>();
builder.Services.AddScoped<AuditoriaRepository>();

// Servicios de negocio
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<SunatService>();
builder.Services.AddScoped<AuditoriaService>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

Console.WriteLine("HASH ADMIN: " + BCrypt.Net.BCrypt.HashPassword("Admin123!"));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseMiddleware<AuditoriaMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();