using Microsoft.EntityFrameworkCore;
using server.internals.dbMigrations;
using server.internals.dbServices;
using server.internals.websocketServices;
using System.Text.Json;
using System.Text.Json.Serialization;

DotNetEnv.Env.Load("./.env");

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddDbContext<MessengerDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddMvc()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
builder.WebHost.UseUrls("http://localhost:3000");
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ProfilePictureService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddCors();

var app = builder.Build();
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
};
app.UseWebSockets(webSocketOptions);
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000", "http://localhost:5173"));
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Map("/chat/{id}", WebsocketChatHandler.HandleChat);
app.Run();
