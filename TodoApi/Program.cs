using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TodoApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// קריאת משתני סביבה
builder.Configuration.AddEnvironmentVariables();

// הוספת Authentication ושימוש ב-JWT
var jwtSecretKey = builder.Configuration["Jwt:Key"] 
                   ?? throw new InvalidOperationException("Missing JWT Key in configuration");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
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
            IssuerSigningKey = key
        };
    });

// הוספת Authorization
builder.Services.AddAuthorization();

// הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Authorization"); // הוספת כותרות לחשיפה אם נדרש
    });
});

// הוספת DbContext לשירותים
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.36-mysql")));

// הוספת Swagger עם תמיכה באימות
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter a valid token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// הוספת שירותי MVC
builder.Services.AddControllers();

var app = builder.Build();

// שימוש ב-Swagger
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// שימוש ב-CORS
app.UseCors("AllowAll");

// שימוש ב-Authentication ו-Authorization
app.UseAuthentication();
app.UseAuthorization();

// טיפול בשגיאות גלובלי
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext http) =>
{
    var error = http.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Problem(error?.Message ?? "An error occurred");
});

// פונקציית לוגין להפקת טוקן
app.MapPost("/login", async (User login, IConfiguration config, ToDoDbContext dbContext) =>
{
    try
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
        if (user == null) return Results.Unauthorized();

        if (user.PasswordHash != login.PasswordHash) return Results.Unauthorized();

        var jwtSecretKey = config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtSecretKey))
        {
            return Results.Problem("JWT SecretKey is missing in the configuration.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, login.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return Results.Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}");
    }
});

// הוספת Route לשליפת כל המשימות (דורש אימות)
app.MapGet("/items", async (ToDoDbContext context) =>
{
    return await context.Items.ToListAsync();
}).RequireAuthorization();

// הוספת Route להוספת משימה חדשה (דורש אימות)
app.MapPost("/items", async (ToDoDbContext context, Item newItem) =>
{
    if (string.IsNullOrEmpty(newItem.Name))
    {
        return Results.BadRequest("Name is required.");
    }

    context.Items.Add(newItem);
    await context.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
}).RequireAuthorization();

// הוספת Route לעדכון משימה (דורש אימות)
app.MapPut("/items/{id}", async (ToDoDbContext context, int id, Item updatedItem) =>
{
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound();
    }

    existingItem.Name = updatedItem.Name;
    existingItem.IsComplete = updatedItem.IsComplete;

    await context.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// הוספת Route למחיקת משימה (דורש אימות)
app.MapDelete("/items/{id}", async (ToDoDbContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();
    }

    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
app.MapGet("/", () => "Todo API is running. Use the endpoints.");
// הפעלת האפליקציה
// app.Run();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
