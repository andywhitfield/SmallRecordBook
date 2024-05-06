using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using SmallRecordBook.Web.Repositories;
using SmallRecordBook.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/signin");
    options.Conventions.AllowAnonymousToPage("/signinverify");
    options.Conventions.AllowAnonymousToPage("/signout");
});
builder.Services.AddTransient<IUserService, UserService>();

builder.Services
    .AddHttpContextAccessor()
    .ConfigureApplicationCookie(c => c.Cookie.Name = "smalllister")
    .AddAuthentication(o => o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/signin";
        o.LogoutPath = "/signout";
        o.Cookie.Name = "smallrecordbook";
        o.Cookie.HttpOnly = true;
        o.Cookie.MaxAge = TimeSpan.FromDays(7);
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.IsEssential = true;
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
    });
builder.Services
    .AddDataProtection()
    .SetApplicationName(typeof(Program).Namespace ?? "")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));
builder.Services.Configure<CookiePolicyOptions>(o =>
{
    o.CheckConsentNeeded = context => false;
    o.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services
    .AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(5))
    .AddFido2(options =>
    {
        options.ServerName = "Small:RecordBook";
        options.ServerDomain = builder.Configuration.GetValue<string>("FidoDomain");
        options.Origins = [builder.Configuration.GetValue<string>("FidoOrigins")];
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    scope.ServiceProvider.GetRequiredService<ISqliteDataContext>().Migrate();

app.Run();

public partial class Program { }