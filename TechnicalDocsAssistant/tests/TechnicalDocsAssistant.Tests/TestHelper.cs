using Microsoft.Extensions.Configuration;
using System.IO;

namespace TechnicalDocsAssistant.Tests
{
    public static class TestHelper
    {
        private static IConfiguration? _configuration;

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("testsettings.json", optional: false);

                    _configuration = builder.Build();
                }

                return _configuration;
            }
        }

        public static string GetSupabaseUrl()
        {
            return Configuration["SupabaseSettings:Url"] ?? 
                throw new InvalidOperationException("Supabase URL not found in test settings");
        }

        public static string GetSupabaseKey()
        {
            return Configuration["SupabaseSettings:Key"] ?? 
                throw new InvalidOperationException("Supabase key not found in test settings");
        }
    }
}
