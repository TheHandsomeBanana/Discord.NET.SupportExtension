using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using HB.NETF.Discord.NET.Toolkit.EntityService.Merged;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Discord.NET.Toolkit.TokenService;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.Services.Security.Cryptography.Settings;
using HB.NETF.VisualStudio.UI;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Helper {
    public static class GenerateHelper {
        public static TokenModel[] GetTokens(IDiscordTokenService tokenService, ConfigureServerImageModel model, ILogger logger) {
            if (model.SaveTokens) {
                switch (model.TokenEncryptionMode) {
                    case EncryptionMode.AES:
                        AesKey tokenKey = GenerateHelper.HandleAesKeyExtractorUI(model.TokenKeyIdentifier, "Token", logger);
                        if (tokenKey == null)
                            return null;

                        return tokenService.DecryptTokens(model.Tokens, model.TokenEncryptionMode.Value, tokenKey);
                    case EncryptionMode.WindowsDataProtectionAPI:
                        return tokenService.DecryptTokens(model.Tokens, model.TokenEncryptionMode.Value);
                }
            }
            else {
                TokenEntryModel tokenEntry = new TokenEntryModel();
                TokenEntryView view = new TokenEntryView { DataContext = new TokenEntryViewModel(tokenEntry) };
                UIHelper.Show(view);
                if (tokenEntry.IsCanceled)
                    return null;

                return tokenEntry.Tokens.Select(f => new TokenModel("Directly provided token", f)).ToArray();
            }

            return null;
        }

        public static void ManipulateMergedEntityServiceDataEncrypt(IMergedDiscordEntityService entityService, ConfigureServerImageModel model, ILogger logger, out bool cancel) {
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

        public static AesKey HandleAesKeyExtractorUI(Guid? id, string name, ILogger logger) {
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
}
