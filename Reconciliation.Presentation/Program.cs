using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reconciliation.Domain.Entities;
using Reconciliation.Infrastructure.Data;
using Reconciliation.Infrastructure.Extensions;
using Reconciliation.Presentation.Extensions;
using Reconciliation.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure Serilog using the Infrastructure layer's extension
builder.Host.UseCustomSerilog();
builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed initial data if needed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Apply migrations
        context.Database.Migrate();

        // Seed data
        await SeedData.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
