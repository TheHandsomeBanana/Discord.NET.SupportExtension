using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.TokenService;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.Services.Security.Cryptography.Settings;
using HB.NETF.VisualStudio.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Helper {
    internal static class InteractionHelper {
        public static InteractionMessages Messages { get; } = new InteractionMessages();
        public static void MapDataEncryptToEntityService(IDiscordEntityService entityService, ConfigureServerImageModel model, ILogger logger, out bool cancel) {
            cancel = false;
            if (model.EncryptData) {
                switch (model.DataEncryptionMode) {
                    case EncryptionMode.AES:
                        AesKey dataKey = HandleAesKeyExtractorUI(model.DataKeyIdentifier, "Data", logger);
                        if (dataKey == null) {
                            cancel = true;
                            return;
                        }

                        entityService.ManipulateStream(o => o.UseBase64()
                        .UseCryptography(EncryptionMode.AES)
                        .ProvideKey(dataKey)
                        .Set());
                        break;
                    case EncryptionMode.WindowsDataProtectionAPI:
                        entityService.ManipulateStream(o => o.UseBase64()
                        .UseCryptography(EncryptionMode.WindowsDataProtectionAPI)
                        .Set());
                        break;
                }
            }
        }
        public static string GetDecryptedToken(IDiscordTokenService tokenService, ConfigureServerImageModel model, ILogger logger) {
            if (model.SaveToken) {
                switch (model.TokenEncryptionMode) {
                    case EncryptionMode.AES:
                        AesKey tokenKey = HandleAesKeyExtractorUI(model.TokenKeyIdentifier, "Token", logger);
                        if (tokenKey == null)
                            return null;

                        return tokenService.DecryptToken(model.Token, model.TokenEncryptionMode.Value, tokenKey);
                    case EncryptionMode.WindowsDataProtectionAPI:
                        return tokenService.DecryptToken(model.Token, model.TokenEncryptionMode.Value);
                }
            }
            else {
                TokenEntryModel tokenEntry = new TokenEntryModel();
                TokenEntryView view = new TokenEntryView { DataContext = new TokenEntryViewModel(tokenEntry) };
                UIHelper.Show(view);
                if (tokenEntry.IsCanceled)
                    return null;

                return tokenEntry.Token;
            }

            return null;
        }
        private static AesKey HandleAesKeyExtractorUI(Guid? id, string name, ILogger logger) {
            KeyEntryModel keyEntry = new KeyEntryModel(name);
            KeyEntryView view = new KeyEntryView() { DataContext = new KeyEntryViewModel(keyEntry) };
            logger.LogInformation($"Requesting aes key for {name}.");
            UIHelper.Show(view);
            if (keyEntry.Key == null) {
                logger.LogInformation("Request cancelled.");
                return null;
            }

            if (!keyEntry.Key.Identify(id.GetValueOrDefault())) {
                string error = $"Wrong key for {name} provided.";
                UIHelper.ShowError(error, "Wrong key");
                logger.LogError(error);
                return null;
            }

            return keyEntry.Key.Reference;
        }
    }

    internal class InteractionMessages {
        // Server Collection
        public string ServerCollectionLoaded = "ServerCollection loaded";
        public string ServerCollectionNotLoaded = "ServerCollection not loaded";
        public string ServerCollectionLoadedFor(string project) => $"{ServerCollectionLoaded} for {project}";
        public string ServerCollectionNotLoadedFor(string project) => $"{ServerCollectionNotLoaded} for {project}";

        // Configuration
        public string ConfigurationFound = "Configuration file found";
        public string ConfigurationNotFound = "No configuration file found";
        public string ConfigurationFoundFor(string project) => $"{ConfigurationFound} for {project}";
        public string ConfigurationNotFoundFor(string project) => $"{ConfigurationNotFound} for {project}";


        // Data
        public string AesRequest = "Requesting aes key";
        public string AesRequestFor(string name) => $"Requesting aes key for {name}";
        public string CouldNotRetrieveAesKey = "Could not retrieve aes key for data decryption";
        public string AesRequestCancelled = "Request for aes key cancelled";
        public string WrongKeyProvided = "Wrong key provided";
        public string WrongKeyProvidedFor(string name) => $"Wrong key provided for {name}";

        // Common
        public string GenerateNewServerImage = "Generate new server image";



    }
}
