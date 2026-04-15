using Enquiry.Data.Model;
using Enquiry.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// services

builder.Services.AddControllers();

// Register LookupService as Singleton
builder.Services.AddSingleton<ILookupService, LookupService>();


// Database Configuration
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

// JWT Configuration 
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);
var secret = jwtSettings.Get<JwtSettings>()!.Secret;
var key = Encoding.UTF8.GetBytes(secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Get<JwtSettings>()!.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Get<JwtSettings>()!.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});


// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "allowCors",
    builder =>
        {
            builder.WithOrigins("https://localhost:4200", "http://localhost:4200", "https://enquiryapi-e7h5g3ggeebwbgc3.eastasia-01.azurewebsites.net")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

//builder.Services.AddDbContext<EnquiryDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("EnquiryDbCon")));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEnquiryService, EnquiryService>();

// configuring OpenAPI
builder.Services.AddOpenApi();

// Swagger enabled for all environments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//Swagger always available
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("allowCors");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
