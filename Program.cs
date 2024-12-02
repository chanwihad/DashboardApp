using DashboardApp.Controllers;
using DashboardApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
    });

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5117"); 
});

builder.Services.AddHttpClient<UserApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5117"); 
});

builder.Services.AddHttpClient<RoleApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5117"); 
});

builder.Services.AddHttpClient<MenuApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5117"); 
});

builder.Services.AddHttpClient<AuthApiClient>();
builder.Services.AddHttpClient<ProductApiClient>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PermissionHelper>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
