using HSProject.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? customConfigFilePath = builder.Configuration["CustomConfigFilePath"];

if (customConfigFilePath != null) {
    builder.Configuration.AddJsonFile(customConfigFilePath, optional: true, reloadOnChange: true);
}

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddScoped<BlacklistService>()
                .AddScoped<ManifestImporterService>();

WebApplication app = builder.Build();

app.MapControllers();

app.Run();
