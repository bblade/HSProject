using HSProject.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddScoped<BlacklistService>()
                .AddScoped<ManifestImporterService>();

WebApplication app = builder.Build();

app.MapControllers();

app.Run();
