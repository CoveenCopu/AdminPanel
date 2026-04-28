using AdminPanel.Data;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//
// Database
//
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//
// MVC
//
builder.Services.AddControllersWithViews();

//
// Authentication (Cookie Auth)
//
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

//
// Authorization
//
builder.Services.AddAuthorization();

//
// Firebase Admin SDK
// Husk at placere serviceAccountKey.json i projektets root
//
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("serviceAccountKey.json")
});

var app = builder.Build();

//
// Pipeline
//
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

//
// VIGTIGT:
// Authentication skal komme før Authorization
//
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();