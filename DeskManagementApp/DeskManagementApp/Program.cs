using DeskManagementApp.Data;
using DeskManagementApp.Models;
using DeskManagementApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Configuration;
using System.Text;
using DeskManagementApp.Hubs;
using System.Reflection.Metadata;
using Humanizer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System;
using DeskManagementApp.Helpers;
using DeskManagementApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var secretKey = builder.Configuration["SecretKey"];

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DeskManagementContext") ?? throw new InvalidOperationException("Connection string 'DeskManagementContext' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<UserManager<IdentityUser>>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
//builder.Services.AddScoped<DatabaseHelper>();

builder.Services.AddControllers();

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddSignalR();

builder.Services.AddMemoryCache();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "ToDo API",
            Description = "An ASP.NET Core Web API for managing ToDo items",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Example Contact",
                Url = new Uri("https://example.com/contact")
            },
            License = new OpenApiLicense
            {
                Name = "Example License",
                Url = new Uri("https://example.com/license")
            }
        });
    });

builder.Services.AddAuthentication(
    opts=>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer((options) =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            )
        };
    }
);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapFallbackToFile("index.html");

app.MapControllers().RequireAuthorization();

app.UseCors("AllowOrigin");

app.UseAuthentication();
app.UseAuthorization();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

using (var serviceScope = app.Services.CreateScope())
{

    var serviceProvider = serviceScope.ServiceProvider;
    using var ctx = serviceProvider.GetService<ApplicationDbContext>();
    await ctx!.Database.MigrateAsync();
    var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
    var rolesToSeed = new IdentityRole[]
    {
        new IdentityRole(){ Name = "User", NormalizedName = "USER"},
        new IdentityRole(){ Name = "DeskAdmin", NormalizedName = "DESKADMIN"},
        new IdentityRole(){ Name = "Admin", NormalizedName = "ADMIN"},
    };
    var roleNames = roleManager!.Roles.Select(r => r.Name).ToList();

    foreach (var role in rolesToSeed)
    {
        if (!roleNames.Contains(role.Name))
        {
            await roleManager.CreateAsync(role);
        }
    }
}

//using (var serviceScope = app.Services.CreateScope())
//{
//    var services = serviceScope.ServiceProvider;
//    var databaseHelper = services.GetRequiredService<DatabaseHelper>();
//    databaseHelper.CreateInitialUser(services).GetAwaiter().GetResult();    
//}

app.UseEndpoints(endpoints =>
{
    app.MapHub<ReservationHub>("/api/reservationHub");
    app.MapHub<DeskHub>("/api/deskHub");
    app.MapHub<EmployeeHub>("/api/employeeHub");
});

app.Map("/api", app =>
{
    app.UseMiddleware<SystemConfiguredMiddleware>();
});

app.Map("/config/create", app =>
{
    app.UseMiddleware<SystemNotConfiguredMiddleware>();
});

app.Run();
