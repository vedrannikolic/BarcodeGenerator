using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QRCodeGen.Services;
using QRCodeGenerator.Hubs;
using QRCodeGenerator.Middlewares;
using QrCoreGen;
using QrCoreGen.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracija JWT
var settings = builder.Configuration.GetSection("2dBarcode");
var key = Encoding.UTF8.GetBytes(settings["JwtKey"]);

builder.Services.AddMemoryCache();

// Dodavanje MVC Controllers sa Views
builder.Services.AddControllersWithViews();

// Konfiguracija autentifikacije JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings["JwtIssuer"],
            ValidAudience = settings["JwtAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Dodavanje usluga za QR kodove
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBarcodeHandler, BarcodeHandler>();

builder.Services.Configure<QrCoreGenOptions>(options => 
    builder.Configuration.GetSection("2dBarcode").Bind(options));

// Dodavanje SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Konfiguracija middleware-a
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();


app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<AuthenticationHub>("/authHub");
});

app.Run();
