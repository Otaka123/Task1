using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProject.Extensions
{
    public class LanguageHelper
    {
        public const string ArabicCulture = "ar-AE";
        public const string EnglishCulture = "en-AE";
        public const string ArabicText = "العربية";
        public const string EnglishText = "English";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;
        private readonly ILogger<LanguageHelper> _logger;

        public LanguageHelper(
            IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
          ILogger<LanguageHelper> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _logger = logger;
        }

        public string GetCurrentLanguageText()
        {
            var currentCulture = CultureInfo.CurrentCulture.Name;
            return currentCulture.StartsWith("ar") ? EnglishText : ArabicText;
        }

        public string GenerateLanguageLink()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("HttpContext is null");
                    return "/";
                }

                var currentCulture = CultureInfo.CurrentCulture.Name;
                var targetCulture = currentCulture.StartsWith("ar") ? EnglishCulture : ArabicCulture;
                var returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;

                var link = _urlHelper.Action("ChangeLanguage", "Language", new
                {
                    area = "Public",
                    culture = targetCulture,
                    returnUrl = returnUrl
                });

                _logger.LogInformation($"Generated language link: {link}");
                return link ?? "/";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating language link");
                return "/";
            }
        }
    }
}
