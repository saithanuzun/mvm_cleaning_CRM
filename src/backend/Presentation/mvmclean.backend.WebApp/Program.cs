using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using mvmclean.backend.Application;
using mvmclean.backend.Infrastructure;
using mvmclean.backend.Infrastructure.Persistence;
using mvmclean.backend.Infrastructure.Seeding;


Env.Load();  // Loads from project root by default

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddAuthentication()
    .AddCookie("AdminCookie", options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(60);
    })
    .AddCookie("ContractorCookie", options =>
    {
        options.LoginPath = "/Contractor/Login";
        options.AccessDeniedPath = "/Contractor/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(60);
    });

builder.Services.AddInfrastructureRegistration();
builder.Services.AddApplicationRegistration();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MVMdbContext>();

    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/Status/{0}");

app.UseResponseCompression();


app.UseHttpsRedirection();

// Static files with 1-year cache for faster repeat visits
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 1 year with immutable directive
        ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
    }
});

var distPath = Path.Combine(builder.Environment.ContentRootPath, "ClientApp/dist");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(distPath),
    RequestPath = "/shop",
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.Contains("assets"))
        {
            ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Map("/shop", appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var indexPath = Path.Combine(distPath, "index.html");
        if (System.IO.File.Exists(indexPath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexPath);
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("index.html not found");
        }
    });
});

// Fallback should be LAST
app.MapFallbackToController(
    action: "GetBySlug",
    controller: "SeoPage");

app.Run();