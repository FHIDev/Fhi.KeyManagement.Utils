using Fhi.Security.Cryptography.CLI.Commands;
using Fhi.Security.Cryptography.CLI.Commands.GenerateJsonWebKey;
using Fhi.Security.Cryptography.CLI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Fhi.Security.Cryptography.CLI
{
    internal class CliHostBuilder(string[] args)
    {
        protected readonly string[] _args = args;

        public IHost BuildHost()
        {
            return Host.CreateDefaultBuilder(_args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddCommandLine(_args);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger, dispose: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IFileHandler, FileHandler>();
                    services.AddTransient<ICommandBuilder, GenerateJsonWebKeyCommandBuilder>();
                    services.AddTransient<JsonWebKeyGeneratorHandler>();
                    services.AddTransient<ICommandBuilder, InvalidCommandBuilder>();
                    ConfigureServices(context, services);
                })
                .Build();
        }

        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
        }
    }
}
