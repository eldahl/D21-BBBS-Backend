using Microsoft.EntityFrameworkCore;
using BBBSBackend.DBModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using BBBSBackend;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add singleton reference to the configuration for injection into the controllers.
// This is needed in the first iteration for accessing issuer, audience, and key values of JWT tokens,
// in the form of dependency injection into the controllers that need to handle JWT tokens.
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<ReservationManager>();
builder.Services.AddSingleton<EmailDispatcher>();


// Database context
builder.Services.AddDbContext<BbbsContext>(options => {
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

// JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Start the singletons as a required services so that they are
// run at the start of the application instead of on first usage.
// ---
// Create variables so that the services don't go out of scope and get garbage collected
var rm = app.Services.GetRequiredService<ReservationManager>();
var ed = app.Services.GetRequiredService<EmailDispatcher>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();    
}

// Disable CORS for given origins
app.UseCors(policy => {
    policy.WithOrigins("http://localhost", "http://localhost:3000", "http://123.123.123.123", "http://123.123.123.123:3000", "http://booking.illerslot.dk")
    .AllowAnyMethod()
    .AllowAnyHeader();
});

// Not at the current moment
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
