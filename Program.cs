using Lab3.Models.Db;
using Lab3.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Csi402dbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.AddSingleton<PlatformPolicyService>();
builder.Services.AddSingleton<PasswordSecurityService>();
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton<ReviewService>();
builder.Services.AddSingleton<BatchOrderService>();
builder.Services.AddSingleton<PromotionService>();
builder.Services.AddSingleton<MembershipService>();
builder.Services.AddSingleton<MenuAddonService>();
builder.Services.AddSingleton<OrderIncidentService>();
builder.Services.AddSingleton<WalletService>();
builder.Services.AddSingleton<CustomerTrustService>();
builder.Services.AddSingleton<RiderTrustService>();
builder.Services.AddSingleton<RiderLocationService>();
builder.Services.AddSingleton<SupportTicketService>();
builder.Services.AddScoped<FinanceWorkspaceService>();
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("FinanceAccess", policy => policy.RequireRole("Accountant"));
});

var app = builder.Build();

await AccountSeeder.EnsureDefaultAccountantAsync(app.Services, app.Logger);

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
