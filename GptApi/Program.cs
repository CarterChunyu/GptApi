using GptApi.Data;
using GptApi.DBServices;
using GptApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ����
builder.Services.AddCors(options =>
{
    options.AddPolicy("policy1", builder =>
    {
        builder.WithOrigins("*")  
            .AllowAnyHeader()
            .AllowAnyMethod().WithExposedHeaders("*");
    });
});


// jwt ����
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = TokenParameter.Issuer,
                ValidAudience = TokenParameter.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenParameter.Secret))
            };
        });

// swagger ��J
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "��J�榡�� Bearer Jwt�r��",

        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    //�K�[�w���n�D
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme{
                Reference =new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id ="Bearer"
                }
            },new string[]{ }
        }
    });
});

// dbcontext
builder.Services.AddDbContext<GptContext>(optionsBuilder =>
{
    ConfigurationManager config = builder.Configuration;
    var status = config["Server:Status"];
    var encodeConn = status == "local" ? config.GetConnectionString("localDB") :
        config.GetConnectionString("remoteDB");
    DesCrytoHelper.TryDesDecrypt(encodeConn, config["Des:k"], config["Des:iv"], out string connStr);

    optionsBuilder.UseSqlServer(connStr, options => options.EnableRetryOnFailure(
            maxRetryCount:5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        ));
});

// service
builder.Services.AddScoped<AuthService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseCors("policy1");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
