using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SimulationProject.Data;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Helper;
using SimulationProject.Helper.TerraformHelper;
using SimulationProject.Services;
using SimulationProject.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSimulationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateSimulationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SecurityQuestionsAndAnswersValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PasswordResetValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PasswordUpdateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<QuestionsUpadateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileValidator>();
builder.Services.Configure<GMailSettings>(builder.Configuration.GetSection("Gmail"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<AthService>();
builder.Services.AddScoped<ILinkService<UserDto>, LinkService>();
builder.Services.AddScoped<ILinkService<SimulationDTO>, SimLinkService>();
builder.Services.AddScoped<NavLinkService>();
builder.Services.AddScoped<IGMailService, GMailService>();
builder.Services.AddScoped<SimulationRunService>();
builder.Services.AddScoped<PollingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", builder =>
    {
        builder.WithOrigins("https://127.0.0.1:5500")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
//builder.WebHost.UseWebRoot("wwwroot");

// Register DbContext with SQL Server provider
builder.Services.AddDbContext<SimSaasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Appsettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Appsettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Appsettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };

        // Read JWT from HttpOnly cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwtCookie"];
                var handler = new JwtSecurityTokenHandler();
                if (!string.IsNullOrEmpty(token))
                {
                    if (handler.CanReadToken(token)){
                        context.Token = token;
                    }               
                    else
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\": \"Invalid token format.\"}");
                    }
            }

                return Task.CompletedTask;
            },

                OnTokenValidated = async context =>
                {
                    //var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var jti = context.Request.Cookies["jwtCookie"];
                    if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
                    {
                        context.Fail("Missing token claims");
                        return;
                    }
               
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<SimSaasContext>();

                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Userid == Int32.Parse(userId));

                    if (user == null || user.Jwtid != jti)
                    {
                        context.Fail("Token is no longer valid.");
                    }
                }
        };
    });

builder.Services.AddAuthorization();

//builder.Services.Configure<CookiePolicyOptions>(options =>
//{
//    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
//});


//Add user secrets for local development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontEnd");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCookiePolicy();
app.UseAuthentication();

// Custom middleware only for NavigationController
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/navigation", StringComparison.OrdinalIgnoreCase))
    {
        
        var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if ((result?.Succeeded == true) && (result?.Principal != null))
        {
            context.User = result.Principal;
        }
    }

    await next();
});
app.UseAuthorization();

app.MapControllers();

app.Run();
