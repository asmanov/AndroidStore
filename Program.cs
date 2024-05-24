using AndroidStore.Data;
using AndroidStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using static AndroidStore.Data.UsersContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.OAuth;
using System;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("UsersConnection");
builder.Services.AddDbContext<UsersContext>(options =>
    options.UseSqlServer(connection ?? throw new InvalidOperationException("Connection string 'UsersContext' not found.")));
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // ?????????, ????? ?? ?????????????? ???????? ??? ????????? ??????
        ValidateIssuer = true,
        // ??????, ?????????????? ????????
        ValidIssuer = AuthOptions.ISSUER,
        // ????? ?? ?????????????? ??????????? ??????
        ValidateAudience = true,
        // ????????? ??????????? ??????
        ValidAudience = AuthOptions.AUDIENCE,
        // ????? ?? ?????????????? ????? ?????????????
        ValidateLifetime = true,
        // ????????? ????? ????????????
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        // ????????? ????? ????????????
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "For Android API", Description = "Making the Pizzas you love", Version = "v1" });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "For Android API V1");
    });
}

app.MapGet("/", () => "Hello World!");
app.MapPost("/login", (UsersContext usersContext, User loginData) =>
{
    // ??????? ???????????? 
    User? person = usersContext.User.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);
    // ???? ???????????? ?? ??????, ?????????? ????????? ??? 401
    if (person is null) return Results.Unauthorized();

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
    // ??????? JWT-?????
    var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    // ????????? ?????
    var response = new
    {
        access_token = encodedJwt,
        username = person.Email
    };

    return Results.Json(response);
});
app.MapPost("/singup", async (UsersContext usersContext, User user) => { 
    await usersContext.User.AddAsync(user);
    await usersContext.SaveChangesAsync();
    return Results.Created($"/user/{user.Id}", user);
});

app.Run();

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // ???????? ??????
    public const string AUDIENCE = "MyAuthClient"; // ??????????? ??????
    const string KEY = "mysupersecret_secretsecretsecretkey!123";   // ???? ??? ????????
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}