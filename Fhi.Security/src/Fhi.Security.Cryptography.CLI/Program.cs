using Fhi.Security.Cryptography.CLI.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Fhi.Security.Cryptography.CLI
{
    /// <summary>
    /// Executable Program for Security.Cryptography.CLI
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var rootCommand = BuildRootCommand(new CliHostBuilder(args));

            var invocationConfig = new InvocationConfiguration
            {
                // Setting this to false enables us to create our own try catch exception handler
                EnableDefaultExceptionHandler = false
            };

            try
            {
                var parseResult = rootCommand.Parse(args);

                if (parseResult.Action is ParseErrorAction parseError)
                {
                    parseError.ShowTypoCorrections = false; // Gives the user typo suggestions for options
                    parseError.ShowHelp = true; // Shows output of command --help if parsing fails
                }

                if (parseResult.Errors.Count > 0)
                {
                    foreach (var error in parseResult.Errors)
                    {
                        Log.Logger.Error("Error trying to run command: {Message}", error.Message);
                    }
                }

                return await parseResult.InvokeAsync(invocationConfig);
            }
            catch (Exception e)
            {
                Log.Logger.Error($"Error occured during command run: {e.Message}");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        internal static RootCommand BuildRootCommand(CliHostBuilder hostBuilder)
        {
            var host = hostBuilder.BuildHost();
            var commandBuilders = host.Services.GetServices<ICommandBuilder>();

            var rootCommand = new RootCommand("Cryptography commands");
            foreach (var builder in commandBuilders)
            {
                var command = builder.Build(host);
                rootCommand.Subcommands.Add(command);
            }

            return rootCommand;
        }
    }
}

