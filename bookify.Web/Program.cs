using bookify.Web.Seeds;
using bookify.Web.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity.UI.Services;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBookifyServices(builder);
//Add Serilog 

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "Deny");
    await next();
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var scopefactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopefactory.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedAsync(roleManager);
await DefaultUsers.SeedAdminUserAsync(userManager);

// hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Bookify Dashboard",
    // IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireAuthorizationFilter("AdminOnly")
    },
});
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
var whatsAppClient = scope.ServiceProvider.GetRequiredService<IWhatsAppClient>();
var emailBodyBuilder = scope.ServiceProvider.GetRequiredService<IEmailBodyBuilder>();
var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

var hangfireTasks = new HangfireTasks(dbContext, webHostEnvironment, whatsAppClient, emailBodyBuilder, emailSender);
RecurringJob.AddOrUpdate("Exp_Alert", () => hangfireTasks.PrepareExpirationAlert(), "0 14 * * *");
RecurringJob.AddOrUpdate("Exp_RentalAlert", () => hangfireTasks.RentalsExpirationAlert(), "0 14 * * *");

app.Use(async (context, next) =>
{
    LogContext.PushProperty("UserId", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    LogContext.PushProperty("UserName", context.User.FindFirst(ClaimTypes.Name)?.Value);
    await next();
});
app.UseSerilogRequestLogging();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
