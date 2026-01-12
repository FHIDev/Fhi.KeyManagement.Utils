using Fhi.HelseIdSelvbetjening.Business;
using Fhi.HelseIdSelvbetjening.Business.Models;
using Fhi.HelseIdSelvbetjening.CLI.Commands.Extensions;
using Fhi.HelseIdSelvbetjening.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration
{
    internal class ReadClientSecretExpirationCommandHandler(
        ILogger<ReadClientSecretExpirationCommandHandler> logger,
        IHelseIdSelvbetjeningService helseIdService,
        IFileHandler fileHandler)
    {
        private readonly ILogger<ReadClientSecretExpirationCommandHandler> _logger = logger;
        private readonly IHelseIdSelvbetjeningService _helseIdService = helseIdService;
        private readonly IFileHandler _fileHandler = fileHandler;

        public async Task<int> ExecuteAsync(ReadClientSecretExpirationParameters parameters)
        {
            using (_logger.BeginScope("Reading client secret for ClientId: {ClientId}", parameters.ClientId))
            {
                var privateKey = KeyResolutionExtensions.ResolveKeyValuePathOrString(
                parameters.ExistingPrivateJwk,
                parameters.ExistingPrivateJwkPath,
                "Private Key",
                _logger,
                _fileHandler);

                if (!string.IsNullOrWhiteSpace(privateKey))
                {
                    var result = await _helseIdService.ReadClientSecretExpiration(new ClientConfiguration(
                        parameters.ClientId, privateKey),
                        parameters.AuthorityUrl, parameters.BaseAddress);

                    return result.HandleResponse(
                        onSuccess: value =>
                        {
                            if (value.SelectedSecret != null)
                            {
                                _logger.LogDebug("Kid: {Kid}", value.SelectedSecret.KeyId);
                                if (value.SelectedSecret.ExpirationDate.HasValue)
                                {
                                    var epochTime = ((DateTimeOffset)value.SelectedSecret.ExpirationDate.Value).ToUnixTimeSeconds();
                                    _logger.LogInformation("{EpochTime}", epochTime);
                                    return 0;
                                }
                                else
                                {
                                    _logger.LogWarning("The client secret with Kid: {Kid} does not have an expiration date.", value.SelectedSecret.KeyId);
                                    _logger.LogInformation("No expiration time (Null)");
                                    return 0;
                                }
                            }

                            _logger.LogError("No secret found with matching Kid.");
                            return 1;
                        },
                        onError: (errorResult) =>
                        {
                            var allMessages = string.Join("; ", errorResult.Errors.Select(e => e.ErrorMessageText));
                            _logger.LogError("Details: {Details}", allMessages);
                            return 1;
                        });
                }
                else
                {
                    _logger.LogError("No private key provided. Either ExistingPrivateJwk or ExistingPrivateJwkPath must be specified.");
                    return 1;
                }
            }
        }
    }
}