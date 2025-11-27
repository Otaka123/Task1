using Domain.Common;
using Microsoft.Extensions.Options;

namespace NewProject.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigurePipeline(this WebApplication app)
        {
            // تكوين خطوط HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // تكوين التوطين
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            // Service Locator (إذا كان ضرورياً - يفضل تجنبه إن أمكن)
            ServiceLocator.ServiceProvider = app.Services;

            // خطوط الوسطى
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // تكوين المسارات
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
