using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Localization
{
    public class SqlStringLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> _localizations;
        private readonly string _resourceKey;

        public SqlStringLocalizer(Dictionary<string, string> localizations, string resourceKey)
        {
            _localizations = localizations;
            _resourceKey = resourceKey;
        }

        public LocalizedString this[string name] => new LocalizedString(name, GetString(name));

        public LocalizedString this[string name, params object[] arguments] =>
            new LocalizedString(name, GetString(name));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public string GetString(string key)
        {
            var culture = CultureInfo.CurrentCulture.ToString();
            string computedKey = $"{key}.{culture}";
            if (_localizations.TryGetValue(computedKey, out var result))
            {
                return result;
            }
            return _resourceKey + "." + computedKey;
        }
    }
}
