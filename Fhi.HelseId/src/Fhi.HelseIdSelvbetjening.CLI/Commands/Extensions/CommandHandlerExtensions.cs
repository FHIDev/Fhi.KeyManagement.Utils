using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions
{
    internal static class KeyResolutionExtensions
    {
        /// <summary>
        ///  Check wether or not a key was provided through direct string or through a filepath.
        ///  Returns either then the value from the string or from reading the file.
        /// </summary>
        /// <param name="directValue"></param>
        /// <param name="filePath"></param>
        /// <param name="keyLabel"></param>
        /// <param name="logger"></param>
        /// <param name="fileHandler"></param>
        /// <returns></returns>
        public static string ResolveKeyValuePathOrString(
            string? directValue,
            string? filePath,
            string keyLabel,
            ILogger logger,
            IFileHandler fileHandler)
        {
            if (!string.IsNullOrWhiteSpace(directValue))
            {
                logger.LogInformation("{keyLabel} provided directly.", keyLabel);
                return UnescapeJson(directValue.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                logger.LogInformation("{keyLabel} loaded from file: {filePath}", keyLabel, filePath);
                return fileHandler.ReadAllText(filePath);
            }

            logger.LogWarning("{keyLabel} not provided.", keyLabel);
            return string.Empty;
        }

        /// <summary>
        /// Unescapes terminal/shell-escaped JSON where the entire structure has escaped quotes.
        /// Handles input like: {\"kid\":\"test\"} -> {"kid":"test"}
        /// Does NOT unescape valid JSON that contains escaped chars within values,
        /// e.g., {"d":"quote-\"here"} stays unchanged.
        /// </summary>
        private static string UnescapeJson(string input)
        {
            // Check for {\" pattern (shell-escaped opening brace+quote)
            if (input.StartsWith("{\\\""))
            {
                return input
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\");
            }
            return input;
        }
    }
}