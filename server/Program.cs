using Microsoft.EntityFrameworkCore;
using server.internals.dbMigrations;
using server.internals.dbServices;
using System.Text.Json;
using System.Text.Json.Serialization;

DotNetEnv.Env.Load("./.env");

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddDbContext<MessengerDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddMvc()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
builder.WebHost.UseUrls("http://localhost");
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ProfilePictureService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "localhost_cors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.UseCors("localhost_cors");
app.Run();
