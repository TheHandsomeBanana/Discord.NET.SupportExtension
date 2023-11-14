using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.MEF.CompletionSource;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Logging;
using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.NETF.VisualStudio.Workspace;
using Microsoft.VisualStudio.LanguageServices;
using EnvDTE;

namespace Discord.NET.SupportExtension.Mef.Roslyn.Completion {
    //[ExportCompletionProvider("DiscordCompletionProvider", LanguageNames.CSharp)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class DiscordCompletionProvider : CompletionProvider {
        private readonly VisualStudioWorkspace vsWorkspace;

        public DiscordCompletionProvider() {
            vsWorkspace = WorkspaceHelper.VisualStudioWorkspace;
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context) {
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            if (loggerFactory == null) // Package not loaded => Nullref
                return;

            ILogger<AsyncDiscordCompletionSource> logger = loggerFactory.GetOrCreateLogger<AsyncDiscordCompletionSource>();
            IAsyncDiscordCompletionEngine engine = DIContainer.GetService<IAsyncDiscordCompletionEngine>();

            try {
                if (context.CancellationToken.IsCancellationRequested) {
                    logger.LogInformation("Completion cancelled");
                    return;
                }

                Assumes.Present(engine);
                Stopwatch stopwatch = Stopwatch.StartNew();

                SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
                SyntaxToken triggerToken = (await context.Document.GetSyntaxRootAsync(context.CancellationToken)).FindToken(context.Position);

                IDiscordCompletionItem[] completions = await engine.ProcessCompletionAsync(vsWorkspace.CurrentSolution, semanticModel, triggerToken);

                if (completions.Length > 0)
                    logger.LogInformation($"{completions.Length} completions added in {stopwatch.ElapsedMilliseconds} ms.");

                stopwatch.Stop();


                context.AddItems(completions.Select(e =>
                    CompletionItem.Create(e.DisplayText)
                ));
            }
            catch (Exception ex) {
                logger.LogCritical("Completion context engine failed. " + ex.ToString());
            }

            return;
        }
    }
}
