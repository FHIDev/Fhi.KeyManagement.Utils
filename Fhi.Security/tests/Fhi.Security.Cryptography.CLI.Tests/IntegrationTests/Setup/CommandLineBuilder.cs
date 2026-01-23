using System.CommandLine;

namespace Fhi.Security.Cryptography.CLI.IntegrationTests.Setup
{
    internal class CommandLineBuilder
    {
        public static async Task<int> CommandLineBuilderInvokerAsync(
            ParseResult parseResult,
            InvocationConfiguration? config = null)
        {
            config ??= new InvocationConfiguration();
            config.EnableDefaultExceptionHandler = false;

            try
            {
                return await parseResult.InvokeAsync(config);
            }
            catch (Exception ex)
            {
                config.Error.WriteLine($"Test caught exception: {ex.Message}");
                return 1;
            }
        }
    }
}
