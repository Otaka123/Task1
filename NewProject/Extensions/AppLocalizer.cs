using Application.Common;
using Microsoft.Extensions.Localization;

namespace NewProject.Extensions
{
    public class AppLocalizer : IAppLocalizer
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AppLocalizer(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public string this[string key] => _localizer[key];
    }
}
