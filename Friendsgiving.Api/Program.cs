using Microsoft.EntityFrameworkCore;
using Friendsgiving.Api.Data;
using Friendsgiving.Api.Hubs;
using Friendsgiving.Api.Infrastructure;
using Friendsgiving.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(ConnectionStrings.Resolve(builder.Configuration)));
builder.Services.AddScoped<GatheringService>();

// The web build and Expo dev server run on other origins. The SignalR browser client
// sends credentials by default, which rules out AllowAnyOrigin, so echo the origin back.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();
app.MapHub<FriendsgivingHub>("/hubs/friendsgiving");
app.MapGet("/health", () => "OK");
// Railway sets this for GitHub deploys; deploy.zsh polls it to know when a push is live.
app.MapGet("/version", () => app.Configuration["RAILWAY_GIT_COMMIT_SHA"] ?? "unknown");

app.Run();
