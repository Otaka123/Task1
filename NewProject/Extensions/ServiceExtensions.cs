using Application.Common;
using Application.Interfaces;
using Application.Services.Account;
using Application.Services.Number;
using Application.Services.OTP;
using Application.Services.PurchaseOrders;
using Application.Services.Signatures;
using Application.Services.SMS;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace NewProject.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCustomLocalization(this IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { new CultureInfo("ar"), new CultureInfo("en") };

                options.DefaultRequestCulture = new RequestCulture("ar");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;
                options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            };
            });

            return services;
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            // خدمات التطبيق
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IOTPService, OTPService>();
            services.AddScoped<ISignatureRepository, SignatureRepository>();
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped<IPurchaseOrderNumberService, PurchaseOrderNumberService>();

            // خدمات المساعدة
            services.AddScoped<IAppLocalizer, AppLocalizer>();
            services.AddScoped<LanguageHelper>();

            // خدمات HTTP
            services.AddHttpClient<ISmsService, SmsService>();

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
        {
            services.AddIdentity<Admin, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddAuthorization();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
            });

            return services;
        }
    }
}
