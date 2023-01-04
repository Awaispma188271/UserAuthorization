using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using userAuth.Model;

using userAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt => opt.AddPolicy("CorsPolicy", c =>
{
    c.WithOrigins("http://localhost:4200")
       .AllowAnyHeader()
       .AllowCredentials()
       .AllowAnyMethod();
}));
// Add services to the container.
builder.Services.AddControllers()
           .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddControllersWithViews()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddControllers();
builder.Services.AddDbContext<UserContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("UserConnectionStrings")));
var _dbcontext = builder.Services.BuildServiceProvider().GetService<UserContext>();
builder.Services.AddSingleton<IRefreshTokenGenerator>(provider => new RefreshTokenGenerator(_dbcontext));

// ADD JWT Authentication
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.Events = new JwtBearerEvents();
    o.Events.OnTokenValidated = context =>
    {
        context.Response.StatusCode = 200;
        return Task.CompletedTask;
    };
    o.Events.OnAuthenticationFailed = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    o.Events.OnChallenge = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    o.Events.OnMessageReceived = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    var key = Encoding.UTF8.GetBytes(builder.Configuration["jwtConfig:Key"]);
    o.SaveToken = true;
    

    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["jwtConfig:issuer"],
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["jwtConfig:audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtConfig:Key"]))
    };
});
builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
