using Application.Common;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace NewProject.Extensions.Identity
{
    public static class AuthorizationPoliciesExtensions
    {
        // ✅ 1. إضافة جميع السياسات للنظام
        public static IServiceCollection AddApplicationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                foreach (var policy in Policies.GetAllPolicies())
                {
                    options.AddPolicy(policy, builder =>
                        builder.RequireClaim("Permission", policy));
                }
            });

            return services;
        }

        // ✅ 2. تهيئة الأدوار والمستخدمين الافتراضيين
        public static async Task InitializeRolesAndPoliciesAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Admin>>();
            var policyService = scope.ServiceProvider.GetRequiredService<IPolicyService>();
            // ✅ التحقق من أن التهيئة لم تتم مسبقاً
            var superAdminEmail = "admin@fclub.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);


            if (superAdmin != null)
            {
                // ✅ إذا كان SuperAdmin موجوداً، تخطي التهيئة لمنع التكرار
                return;

            }

            await policyService.InitializePoliciesAsync();

            // ✅ تهيئة الصلاحيات أولاً

            // ✅ إنشاء الأدوار أولاً
            await SeedRolesAsync(roleManager);

            // ✅ ثم إنشاء المستخدمين مع تعيين الصلاحيات المناسبة
            await SeedUsersWithAppropriatePoliciesAsync(userManager);
        }

        // ✅ 3. إنشاء الأدوار فقط
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = {
               "SuperAdmin",
               "Admin",
              "Editor",
               "Viewer",
                "ContentManager",
                "User"
            };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        // ✅ 4. إنشاء المستخدمين مع الصلاحيات المناسبة لكل دور
        private static async Task SeedUsersWithAppropriatePoliciesAsync(UserManager<Admin> userManager)
        {
            // ✅ إنشاء SuperAdmin واحد فقط بجميع الصلاحيات
            await CreateSuperAdminAsync(userManager);

            // ✅ إنشاء المستخدمين الآخرين بصلاحيات محددة حسب الأدوار
            //await CreateOtherAdminsAsync(userManager);
        }

        // ✅ 5. إنشاء SuperAdmin واحد فقط بجميع الصلاحيات
        private static async Task CreateSuperAdminAsync(UserManager<Admin> userManager)
        {
            var superAdminEmail = "admin@fclub.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdmin == null)
            {
                var user = new Admin
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, new[] { "SuperAdmin", "Admin" });

                    // ✅ إضافة جميع الصلاحيات لـ SuperAdmin فقط
                    var allPolicies = Policies.GetAllPolicies();
                    var existingClaims = await userManager.GetClaimsAsync(user);

                    foreach (var policy in allPolicies)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == policy))
                        {
                            await userManager.AddClaimAsync(user, new Claim("Permission", policy));
                        }
                    }
                }
            }
        }

    //    // ✅ 6. إنشاء المستخدمين الآخرين بصلاحيات محددة
    //    private static async Task CreateOtherAdminsAsync(UserManager<Admin> userManager)
    //    {
    //        var random = new Random();

    //        // ✅ تحديث الصلاحيات لتشمل جميع الصلاحيات الجديدة
    //        var rolePermissions = new Dictionary<string, string[]>
    //        {
    //         {
    //        "Admin",
    //        new[] {
    //            // AboutUs / Section
    //            Policies.AboutUs.View, Policies.AboutUs.Edit,
                
    //            // Event
    //            Policies.Event.View, Policies.Event.Create, Policies.Event.Edit, Policies.Event.Delete,
                
    //            // StaticPages
    //            Policies.StaticPages.Location, Policies.StaticPages.Quality,
    //            Policies.StaticPages.Privacy, Policies.StaticPages.termsCondtions,
                
    //            // Course
    //            Policies.Course.View, Policies.Course.Create, Policies.Course.Edit, Policies.Course.Delete,
                
    //            // Album
    //            Policies.Album.View, Policies.Album.Create, Policies.Album.Edit, Policies.Album.Delete,
                
    //            // Videos
    //            Policies.Videos.View, Policies.Videos.Create, Policies.Videos.Edit, Policies.Videos.Delete,
                
    //            // User & Role Management
    //            Policies.User.Management, Policies.Role.Management,
                
    //            // News
    //            Policies.News.View, Policies.News.Create, Policies.News.Edit, Policies.News.Delete,
                
    //            // Slider
    //            Policies.Slider.View, Policies.Slider.Create, Policies.Slider.Delete,
                
    //            // Website Information
    //            Policies.WebsiteInformation.Management,
                
    //            // ContactUs
    //            Policies.ContactUs.View
    //        }
    //    },
    //    {
    //        "ContentManager",
    //        new[] {
    //            Policies.AboutUs.View, Policies.AboutUs.Edit,
    //            Policies.Event.View, Policies.Event.Create, Policies.Event.Edit,
    //            Policies.StaticPages.Location, Policies.StaticPages.Quality,
    //            Policies.StaticPages.Privacy, Policies.StaticPages.termsCondtions,
    //            Policies.Course.View, Policies.Course.Create, Policies.Course.Edit,
    //            Policies.Album.View, Policies.Album.Create, Policies.Album.Edit,
    //            Policies.Videos.View, Policies.Videos.Create, Policies.Videos.Edit,
    //            Policies.News.View, Policies.News.Create, Policies.News.Edit,
    //            Policies.Slider.View, Policies.Slider.Create, Policies.Slider.Delete,
    //            Policies.WebsiteInformation.Management,
    //            Policies.ContactUs.View
    //        }
    //    },
    //    {
    //        "Editor",
    //        new[] {
    //            Policies.AboutUs.View, Policies.AboutUs.Edit,
    //            Policies.Event.View, Policies.Event.Edit,
    //            Policies.StaticPages.Location, Policies.StaticPages.Quality,
    //            Policies.Course.View, Policies.Course.Edit,
    //            Policies.Album.View, Policies.Album.Edit,
    //            Policies.Videos.View, Policies.Videos.Edit,
    //            Policies.News.View, Policies.News.Edit,
    //            Policies.Slider.View,
    //            Policies.ContactUs.View
    //        }
    //    },
    //    {
    //        "Viewer",
    //        new[] {
    //            Policies.AboutUs.View,
    //            Policies.Event.View,
    //            Policies.Course.View,
    //            Policies.Album.View,
    //            Policies.Videos.View,
    //            Policies.News.View,
    //            Policies.Slider.View,
    //            Policies.ContactUs.View
    //        }
    //    },
    //    {
    //        "User",
    //        new[] {
    //            Policies.AboutUs.View,
    //            Policies.Event.View,
    //            Policies.Course.View,
    //            Policies.Album.View,
    //            Policies.Videos.View,
    //            Policies.News.View,
    //            Policies.Slider.View,
    //            Policies.ContactUs.View
    //        }
    //    }
    //};

    //        string[] roleNames = {
    //    "Admin",
    //    "ContentManager",
    //    "Editor",
    //    "Viewer",
    //    "User"
    //};

    //        for (int i = 1; i <= 9; i++)
    //        {
    //            var email = $"admin{i}@fclub.com";
    //            var existingUser = await userManager.FindByEmailAsync(email);

    //            if (existingUser == null)
    //            {
    //                var user = new Admin
    //                {
    //                    UserName = email,
    //                    Email = email,
    //                    FirstName = "Admin",
    //                    LastName = i.ToString(),
    //                    EmailConfirmed = true
    //                };

    //                var result = await userManager.CreateAsync(user, "Admin@123");

    //                if (result.Succeeded)
    //                {
    //                    var randomRole = roleNames[random.Next(0, roleNames.Length)];
    //                    await userManager.AddToRoleAsync(user, randomRole);

    //                    // ✅ إضافة الصلاحيات المناسبة للدور فقط
    //                    if (rolePermissions.ContainsKey(randomRole))
    //                    {
    //                        var permissions = rolePermissions[randomRole];
    //                        foreach (var permission in permissions)
    //                        {
    //                            await userManager.AddClaimAsync(user, new Claim("Permission", permission));
    //                        }
    //                    }

    //                    // ✅ إضافة Claim للدور للمرجعية
    //                    await userManager.AddClaimAsync(user, new Claim("RoleLevel", randomRole));
    //                }
    //            }
    //        }
        //}
    }
}
