using Microsoft.EntityFrameworkCore;
using server.internals.dbMigrations;
using server.internals.dbServices;

DotNetEnv.Env.Load("./.env");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MessengerDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
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
builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson();
var app = builder.Build();
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
};
app.UseWebSockets(webSocketOptions);
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseRouting();
app.MapControllers();
app.Run();