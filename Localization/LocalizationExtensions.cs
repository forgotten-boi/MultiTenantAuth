using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace Localization
{
    public static class LocalizationExtensions
    {
        public static IServiceCollection AddSqlLocalization(this IServiceCollection services,
            Action<SqlLocalizationOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IStringLocalizerFactory, SqlStringLocalizerFactory>();
            services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            if (setupAction != null)
                services.Configure(setupAction);
            return services;
        }

        public static string GetValue<T>(this IStringLocalizer<T> localizer, string key)
        {
            var item = localizer.GetString(key);
            if (item != null) return item.Value;
            return key;
        }
    }
}
