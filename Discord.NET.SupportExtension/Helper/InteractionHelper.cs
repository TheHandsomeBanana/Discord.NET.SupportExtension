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
        public static void MapDataEncryptToEntityService(IDiscordEntityService entityService, ConfigureServerImageModel model, ILogger logger, out bool cancel) {
            cancel = false;
            if (model.EncryptData) {
                switch (model.DataEncryptionMode) {
                    case EncryptionMode.AES:
                        AesKey dataKey = GetAesKeyFromUIInput(model.DataKeyIdentifier, "Data", logger);
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
                        AesKey tokenKey = GetAesKeyFromUIInput(model.TokenKeyIdentifier, "Token", logger);
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
        public static AesKey GetAesKeyFromUIInput(Guid? id, string name, ILogger logger) {
            KeyEntryModel keyEntry = new KeyEntryModel(name);
            KeyEntryView view = new KeyEntryView() { DataContext = new KeyEntryViewModel(keyEntry) };
            logger.LogInformation(InteractionMessages.AesRequestFor(name));
            UIHelper.Show(view);
            if(keyEntry.IsCanceled) {
                logger.LogInformation(InteractionMessages.AesRequestCancelled);
                return null;
            }

            if (keyEntry.Key == null) {
                logger.LogInformation(InteractionMessages.NoKeyFound);
                return null;
            }

            if (!keyEntry.Key.Identify(id.GetValueOrDefault())) {
                string error = InteractionMessages.WrongKeyProvidedFor(name);
                UIHelper.ShowError(error, "Wrong key");
                logger.LogError(error);
                return null;
            }

            return keyEntry.Key.Reference;
        }
    }

    internal static class InteractionMessages {
        // Server Collection
        public const string ServerCollectionLoaded = "ServerCollection loaded";
        public const string ServerCollectionNotLoaded = "ServerCollection not loaded";
        public const string ServerCollectionAlreadyLoaded = "ServerCollection already loaded";
        public static string ServerCollectionLoadedFor(string project) => $"{ServerCollectionLoaded} for {project}";
        public static string ServerCollectionNotLoadedFor(string project) => $"{ServerCollectionNotLoaded} for {project}";
        public static string ServerCollectionAlreadyLoadedFor(string project) => $"{ServerCollectionAlreadyLoaded} for {project}";

        // Configuration
        public const string WindowWithEmptyConfiguration = "Window loaded with empty configuration";
        public const string ConfigurationFound = "Configuration file found";
        public const string ConfigurationNotFound = "No configuration file found";
        public static string ConfigurationFoundFor(string project) => $"{ConfigurationFound} for {project}";
        public static string ConfigurationNotFoundFor(string project) => $"{ConfigurationNotFound} for {project}";

        public const string ConfigurationSaved = "Configuration saved";
        public static string ConfigurationSavedTo(string project) => $"Configuration saved to {project}";


        // AES
        public const string AesRequest = "Requesting aes key";
        public static string AesRequestFor(string name) => $"Requesting aes key for {name}";
        public const string CouldNotRetrieveAesKey = "Could not retrieve aes key for data decryption";
        public const string AesRequestCancelled = "Request for aes key cancelled";
        public const string WrongKeyProvided = "Wrong key provided";
        public static string WrongKeyProvidedFor(string name) => $"Wrong key provided for {name}";
        public const string NoKeyFound = "No key found";

        // Token
        public const string GetTokenFailed = "Get token failed";
        public const string TokenSaved = "Token encrypted and saved";
        public const string NoTokenToSave = "No token to save";
        public const string ProvideToken = "Provide a token before saving";
        public const string TokenInvalid = "Token is invalid";
        public const string TokenAndRunLogWillBeRemoved = "Token will be removed and run log cleared";

        // Image Generation
        public const string GenerateImageSuccess = "Server image successfully generated";
        public const string GenerateImageFailure = "Server image generation failed";
        public const string GenerateNewServerImage = "Generate new server image";
        public const string GenerationAborted = "Server image generation aborted";

        // Common
        public const string ConnectionTimeout = "Connection timed out";

    }
}
