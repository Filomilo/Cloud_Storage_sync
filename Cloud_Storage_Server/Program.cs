using System.Security.Claims;
using System.Text;
using Cloud_Storage_Server.Configurations;
using Cloud_Storage_Server.Database;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme,
        },
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(
        new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } }
    );
});
SqliteDataBaseContextGenerator contextGenerator = new SqliteDataBaseContextGenerator();
builder.Services.AddSingleton<IDataBaseContextGenerator, SqliteDataBaseContextGenerator>(provider =>
    contextGenerator
);
AuthService authService = new AuthService(contextGenerator);
builder.Services.AddSingleton<IAuthService>(provider => authService);

//builder.Services.AddDbContext<DatabaseContext>();
WebsocketConnectedController WebsocketConnectedController = new WebsocketConnectedController();
builder.Services.AddSingleton<IWebsocketConnectedController, WebsocketConnectedController>(
    provider => WebsocketConnectedController
);

IServerConfig serverConfig = new ServerConfig();
builder.Services.AddSingleton<IServerConfig>(provider => serverConfig);
FileSystemService fileSystemService = new FileSystemService(serverConfig);
builder.Services.AddSingleton<IFileSystemService>(provider => fileSystemService);
builder.Services.AddSingleton<IFileSyncService>(provider => new FileSyncService(
    fileSystemService,
    WebsocketConnectedController,
    contextGenerator,
    serverConfig
));
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(AuthConfiguration.PrivateKey)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        options.Events = new JwtBearerEvents()
        {
            OnTokenValidated = context =>
            {
                using (AbstractDataBaseContext ctx = contextGenerator.GetDbContext())
                {
                    var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                    var mail = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

                    if (string.IsNullOrEmpty(mail))
                    {
                        context.Fail("Unauthorized: User Mail missing in token.");
                    }

                    if (!UserRepository.DoesUserWithMailExist(ctx, mail))
                    {
                        context.Fail("Unauthorized: User {mail} not exist.");
                    }
                }

                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { });
}

app.UseAuthorization();

app.MapControllers();
var webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(2) };

app.UseWebSockets(webSocketOptions);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var contextGeneratorTmp = services.GetRequiredService<IDataBaseContextGenerator>();
    //context.Database.Migrate();
    using (var context = contextGeneratorTmp.GetDbContext())
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}

app.Run();

public partial class Program { }
