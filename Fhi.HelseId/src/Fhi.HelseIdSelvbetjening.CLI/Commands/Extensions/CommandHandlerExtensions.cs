using System.Text.Json;
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
        /// Unescapes JSON string escape sequences by deserializing the input as a JSON string value.
        /// <see href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding"/>
        /// </summary>
        private static string UnescapeJson(string input)
        {
            try
            {
                return JsonSerializer.Deserialize<string>($"\"{input}\"")!;
            }
            catch (JsonException)
            {
                return input;
            }
        }
    }
}