using bookify.Web.Core.Mapping;
using bookify.Web.Helpers;
using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using ViewToHTML.Extensions;
using WhatsAppCloudApi.Extensions;

namespace bookify.Web.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBookifyServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString!));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            });
            services.AddDataProtection().SetApplicationName(nameof(bookify));

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrinciplalFactory>();
            services.AddControllersWithViews();
            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
            services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));
            services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
            services.AddExpressiveAnnotations();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            services.AddSingleton<IHashids>(_ => new Hashids("f1nd1ngn3m0", minHashLength: 11));
            services.AddWhatsAppApiClient(builder.Configuration);
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();
            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
            services.Configure<AuthorizationOptions>(options => options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AppRoles.Admin);
            }));
            services.AddViewToHTML();
            services.AddMvc(option => option.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            return services;
        }
    }
}
