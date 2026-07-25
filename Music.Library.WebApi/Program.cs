using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Music.Library.Business;
using Music.Library.Business.Abstractions;
using Music.Library.Repository;
using Music.Library.Repository.Abstractions;
using MusicLibrary;
using MusicLibrary.Correlation;
using MusicLibrary.Kafka;
using MusicLibrary.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("ServiceName", "LibraryService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Avvio di LibraryService...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // DbContext
    builder.Services.AddDbContext<LibraryDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IRepository, Repository>();
    builder.Services.AddScoped<IBusiness, Business>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

    builder.Services.AddResilientHttpClients(builder.Configuration);
    builder.Services.AddKafkaProducerService<LibraryKafkaTopics, LibraryProducerService>(builder.Configuration);

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),

                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Music Library API",
            Version = "v1",
            Description = "API per la gestione della libreria musicale"
        });

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Inserisci il token nel formato: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Applica automaticamente le migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        db.Database.Migrate();
    }

    // Deve precedere UseSerilogRequestLogging per far sì che anche la riga di log
    // riassuntiva della richiesta sia arricchita con il CorrelationId
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LibraryService terminato in modo inatteso durante l'avvio");
}
finally
{
    Log.CloseAndFlush();
}