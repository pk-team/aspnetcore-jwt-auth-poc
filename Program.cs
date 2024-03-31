using System.Text;

using App;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GenerateJwtTokenHandler>();

var JwtSettings = new JwtSettings();
builder.Configuration.Bind(nameof(JwtSettings), JwtSettings);
builder.Services.AddSingleton(JwtSettings);

builder.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {

        var jwtOptions = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidAudience = JwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) => {
    Console.WriteLine(context.User?.Identity?.Name ?? "Anonymous");
    return "Hello World!";
});

app.MapGet("/options", ([FromServices] JwtSettings options, HttpContext context) => {
    return options.SecretKey;
});

app.MapPost("/gentoken", ([FromServices] GenerateJwtTokenHandler handler, [FromBody] User user) => {
    var token = handler.Handle(user);
    return Results.Ok(token);
});

app.MapGet("/users", () => {
    // return 2 users
    var users = new List<User> {
        new(
            Name: "John Doe",
            Email: "John@gmail.com",
            Roles: new List<string> { "Admin", "User" }
        ),
        new(
            Name: "Jane Doe",
            Email: "Jane@gmail.com",
            Roles: new List<string> { "User", "Editor" }
        )
    };
    return Results.Ok(users);
}).RequireAuthorization();

app.Run();