using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Localization
{
    public class SqlStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, IStringLocalizer> _resourceLocalizations = new ConcurrentDictionary<string, IStringLocalizer>();
        private readonly SqlLocalizationOptions _options;
        public SqlStringLocalizerFactory(IOptions<SqlLocalizationOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }
        public IStringLocalizer Create(Type resourceSource)
        {
            var key = resourceSource.Name;
            return Create(key);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var key = string.Concat(baseName, location);
            return Create(key);
        }

        private IStringLocalizer Create(string key)
        {
            if (_resourceLocalizations.ContainsKey(key))
                return _resourceLocalizations[key];

            var resources = GetResourcesFromDb(key);
            var sqlStringLocalizer = new SqlStringLocalizer(resources, key);
            return _resourceLocalizations.GetOrAdd(key, sqlStringLocalizer);
        }

        private Dictionary<string, string> GetResourcesFromDb(string resourceName)
        {
            var dict = new Dictionary<string, string>();
            using (var con = new SqlConnection(_options.ConnectionString))
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = @"SELECT CultureName, ResourceKey, ResourceValue 
                                        FROM Localization 
                                        WHERE ResourceName = @ResourceName";
                    cmd.Parameters.AddWithValue("@ResourceName", resourceName);
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var culture = reader.GetString(0);
                            var key = reader.GetString(1);
                            var value = reader.GetString(2);
                            dict.Add($"{key}.{culture}", value);
                        }
                    }
                }
            }
            return dict;
        }
    }
}
