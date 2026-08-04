using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ClinicFlow.Api.Data;
using ClinicFlow.Api.Middleware;
using ClinicFlow.Api.Services;
using ClinicFlow.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services (dependency injection registration)
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// MediatR for domain events
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Middleware pipeline (order matters)
app.UseMiddleware<ExceptionHandlingMiddleware>();  // Global error handling

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect in non-development: the Angular dev server talks to the plain
// HTTP endpoint, and a 307 to an untrusted dev certificate breaks those calls.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// No authentication scheme is configured yet, so UseAuthentication() is omitted --
// calling it without a prior AddAuthentication() throws at startup.
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed demo data on startup (development convenience).
await app.Services.InitialiseDatabaseAsync();

app.Run();