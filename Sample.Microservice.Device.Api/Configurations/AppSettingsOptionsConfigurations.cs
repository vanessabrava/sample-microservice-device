using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Microservice.Device.Infra.CrossCutting.Options;
using System;
using System.IO;
using System.Text;

namespace Sample.Microservice.Device.Api.Configurations
{
    internal static class AppSettingsOptionsConfigurations
    {
        private static string AppSettingsGlobalization => "Globalization";
        private static string AppSettingsErrorLog => "Logs:Error";

        public static WebHostBuilderContext ConfigAppSettingsFiles(this WebHostBuilderContext hostingContext, IConfigurationBuilder configuration)
        {
            if (hostingContext.HostingEnvironment.EnvironmentName != "Development")
                configuration.AddJsonFile("appsettings/appsettings.json".BindEnvironmentVariable(), optional: true, reloadOnChange: true);

            configuration.AddJsonFile($"appsettings/appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            configuration.AddEnvironmentVariables();

            return hostingContext;
        }

        public static IServiceCollection ConfigureAppSettingsOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<ErrorLogOptions>(options => configuration.GetSection(AppSettingsErrorLog).Bind(options));
            services.Configure<GlobalizationOptions>(options => configuration.GetSection(AppSettingsGlobalization).Bind(options));

            return services;
        }

        private static string BindEnvironmentVariable(this string appSettingsDirectoryAndFolder)
        {
            string pathToContentRoot = Directory.GetCurrentDirectory();

            if (!File.Exists(appSettingsDirectoryAndFolder)) throw new FileNotFoundException($"File {appSettingsDirectoryAndFolder} not found.");

            string content = File.ReadAllText(appSettingsDirectoryAndFolder, Encoding.UTF8);

            while (content.Extract("#{", "}#", false, false) != null)
            {
                string environmentVariableKey = content.Extract("#{", "}#", false, false);
                string appSettingsKey = "#{" + environmentVariableKey + "}#";
                var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableKey);

                if (!string.IsNullOrWhiteSpace(environmentVariableValue))
                    content = content.Replace(appSettingsKey, environmentVariableValue);
                else
                    content = content.Replace(appSettingsKey, "![NOT_FOUND]!");
            }

            File.WriteAllText(Path.Combine(pathToContentRoot, appSettingsDirectoryAndFolder), content);

            return appSettingsDirectoryAndFolder;
        }

        private static string Extract(this string source, string beginDelim, string endDelim, bool caseSensitive = false, bool allowMissingEndDelimiter = false)
        {
            int posIni, posEnd;

            if (string.IsNullOrEmpty(source))
                return null;

            if (caseSensitive)
            {
                posIni = source.IndexOf(beginDelim);
                if (posIni == -1)
                    return null;

                posEnd = source.IndexOf(endDelim, posIni + beginDelim.Length);
            }
            else
            {
                string Lower = source.ToLower();
                posIni = source.IndexOf(beginDelim, 0, source.Length, StringComparison.OrdinalIgnoreCase);
                if (posIni == -1)
                    return null;

                posEnd = source.IndexOf(endDelim, posIni + beginDelim.Length, StringComparison.OrdinalIgnoreCase);
            }

            if (allowMissingEndDelimiter && posEnd == -1)
                return source.Substring(posIni + beginDelim.Length);

            if (posIni > -1 && posEnd > 1)
                return source.Substring(posIni + beginDelim.Length, posEnd - posIni - beginDelim.Length);

            return null;
        }
    }
}
