using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using Venbacodes.Samples.ABAC.Authorization;
using Venbacodes.Samples.ABAC.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(vu =>
{
    vu.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Generate JWT access token with Email, Expiry. Check TestUsers.cs for test emails"
    });

    vu.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

builder.Services.AddScoped<JwtSecurityTokenHandler>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<IAuthorizationPolicyProvider, VUPolicyProvider>();
builder.Services.AddTransient<IAuthorizationHandler, RolesAuthHandler>();
builder.Services.AddTransient<IAuthorizationHandler, PermissionsAuthHandler>();

builder.Services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, VUAuthenticationHandler>("VU", null);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
