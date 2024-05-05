using HSProject.Api.Authentication;
using HSProject.Api.Options;
using HSProject.Api.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services
    .Configure<FileMakerOptions>(
        builder.Configuration.GetSection(nameof(FileMakerOptions)));

builder.Services.AddScoped<FileMakerService>();

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConn")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

builder.Services
            .AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters() {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            });

builder.Services.Configure<SecurityStampValidatorOptions>(options => {
    options.ValidationInterval = TimeSpan.Zero;
});

FileMakerOptions filemakerOptions = builder.Configuration
            .GetSection(nameof(FileMakerOptions))
            .Get<FileMakerOptions>() ?? new();

builder.Services.AddHttpClient("FM", m => { })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
        ServerCertificateCustomValidationCallback = (m, c, ch, e) => true, 
});

WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

using (IServiceScope scope = app.Services.CreateScope()) {
    IdentityDbContext identityContext = scope.ServiceProvider
        .GetRequiredService<IdentityDbContext>();
    identityContext.Database.Migrate();
}

app.MapControllers();

app.Run();
