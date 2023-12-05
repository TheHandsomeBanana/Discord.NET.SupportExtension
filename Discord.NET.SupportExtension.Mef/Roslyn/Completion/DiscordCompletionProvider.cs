using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.MEF.CompletionSource;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Unity;
using HB.NETF.VisualStudio.Workspace;
using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace Discord.NET.SupportExtension.Mef.Roslyn.Completion {
    //[ExportCompletionProvider("DiscordCompletionProvider", LanguageNames.CSharp)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class DiscordCompletionProvider : CompletionProvider {
        private readonly VisualStudioWorkspace vsWorkspace;

        public DiscordCompletionProvider() {
            vsWorkspace = WorkspaceHelper.VisualStudioWorkspace;
            UnityBase.UnityContainer.BuildUp(this);
        }

        [Dependency("DiscordSupportPackage")]
        public IUnityContainer UnityContainer { get; set; }

        public override async Task ProvideCompletionsAsync(CompletionContext context) {
            ILoggerFactory loggerFactory = UnityContainer.Resolve<ILoggerFactory>();
            if (loggerFactory == null) // Package not loaded => Nullref
                return;

            ILogger<AsyncDiscordCompletionSource> logger = loggerFactory.GetOrCreateLogger<AsyncDiscordCompletionSource>();
            IDiscordCompletionEngine engine = UnityContainer.Resolve<IDiscordCompletionEngine>();

            try {
                if (context.CancellationToken.IsCancellationRequested) {
                    logger.LogInformation("Completion cancelled");
                    return;
                }

                Assumes.Present(engine);
                Stopwatch stopwatch = Stopwatch.StartNew();

                SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
                SyntaxToken triggerToken = (await context.Document.GetSyntaxRootAsync(context.CancellationToken)).FindToken(context.Position);

                DiscordCompletionItem[] completions = await engine.ProcessCompletionAsync(vsWorkspace.CurrentSolution, semanticModel, triggerToken);

                context.AddItems(completions.Select(e =>
                    CompletionItem.Create(e.DisplayText)
                ));


                if (completions.Length > 0)
                    logger.LogInformation($"{completions.Length} completions added in {stopwatch.ElapsedMilliseconds} ms.");

                stopwatch.Stop();



            }
            catch (Exception ex) {
                logger.LogCritical("Completion context engine failed. " + ex.ToString());
            }

            return;
        }

        public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options) {
            return char.IsDigit(trigger.Character);
        }
    }
}
