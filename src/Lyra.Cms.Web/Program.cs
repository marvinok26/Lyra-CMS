using OrchardCore.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    // Lets the Default tenant provision itself from appsettings/environment config (see
    // docker-compose.yml) instead of requiring a manual click-through of the setup wizard —
    // not enabled by the `occms` template by default, so it's turned on explicitly here.
    .AddSetupFeatures("OrchardCore.AutoSetup")
;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseOrchardCore();

app.Run();
