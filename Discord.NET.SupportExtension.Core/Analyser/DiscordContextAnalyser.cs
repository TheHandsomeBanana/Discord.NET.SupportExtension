using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class DiscordContextAnalyser : DiscordAnalyserBase, IDiscordContextAnalyser {
        private DiscordBaseCompletionContext context;
        private DiscordChannelContext? channelContext;
        private SyntaxNode contextNode;

        public async Task<DiscordCompletionContext> Run(SyntaxNode node) {
            SyntaxNode currentNode = node;
            if (currentNode.IsKind(SyntaxKind.NumericLiteralExpression))
                currentNode = currentNode.Parent;

            if (InitiateAnalysis(currentNode))
                await ResolveNode(currentNode);

            if (context != DiscordBaseCompletionContext.Undefined)
                return new DiscordCompletionContext(contextNode, context, channelContext);

            return DiscordCompletionContext.Undefined;
        }


        #region Initiate Analysis
        private bool InitiateAnalysis(SyntaxNode trigger) {
            if (trigger is BinaryExpressionSyntax)
                return true;

            if (trigger is ArgumentListSyntax || trigger is ArgumentSyntax)
                return CheckMethodArgumentUsage(trigger);

            return false;
        }

        private bool CheckMethodArgumentUsage(SyntaxNode trigger) {
            int argumentIndex = -1;
            InvocationExpressionSyntax parentInvocation;
            switch (trigger) {
                case ArgumentSyntax argument:
                    if (!argument.Expression.IsKind(SyntaxKind.NumericLiteralExpression))
                        return false;

                    argumentIndex = GetArgumentIndex(argument);
                    parentInvocation = argument.Parent.Parent as InvocationExpressionSyntax;
                    break;
                case ArgumentListSyntax argumentList:
                    if (argumentList.Arguments.Count > 0)
                        argumentIndex = 0;
                    parentInvocation = argumentList.Parent as InvocationExpressionSyntax;
                    break;
                default:
                    return false;
            }

            IMethodSymbol methodSymbol = SemanticModel.GetSymbolInfo(parentInvocation).Symbol as IMethodSymbol;

            if (methodSymbol == null) { // Context can already be found while initiating
                CheckForContext(parentInvocation);
                return false;
            }

            return argumentIndex >= 0 && methodSymbol.Parameters[argumentIndex].Type.ToString() == "ulong"
                    // Linq methods should also trigger context detection => .FirstOrDefault(e => e.Id == )
                    || CheckLinq(methodSymbol, trigger);
        }

        private bool CheckLinq(IMethodSymbol methodSymbol, SyntaxNode trigger) {
            return methodSymbol.IsExtensionMethod
                && methodSymbol.ContainingNamespace.ToString() == "System.Linq"
                && trigger is ArgumentListSyntax argumentList
                && argumentList.Arguments.ElementAtOrDefault(0)?.Expression is SimpleLambdaExpressionSyntax lambda
                && lambda.Body is BinaryExpressionSyntax;
        }
        #endregion

        #region Analysis
        private async Task ResolveNode(SyntaxNode node) {
            switch (node) {
                case InvocationExpressionSyntax invocation:
                    CheckForContext(invocation);
                    break;
                case BinaryExpressionSyntax binary when binary.Kind() == SyntaxKind.EqualsExpression || binary.Kind() == SyntaxKind.NotEqualsExpression:
                    await ResolveNode(binary.Left);
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    await ResolveNode(memberAccess.Expression);
                    break;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    await ResolveNode(conditionalAccess.Expression);
                    break;
                case IdentifierNameSyntax identifier:
                    CheckForContext(identifier);
                    break;
                case SwitchStatementSyntax switchStatement:
                    await ResolveNode(switchStatement.Expression);
                    break;
                case ArgumentSyntax argument:
                    await ResolveArgument(argument);
                    break;
                case ArgumentListSyntax argumentList:
                    await ResolveNode(argumentList.Parent);
                    break;
            }
        }

        private async Task ResolveArgument(ArgumentSyntax argument) {
            int argumentIndex = GetArgumentIndex(argument);
            if (!(argument.Parent.Parent is InvocationExpressionSyntax invocation))
                return;

            IMethodSymbol methodSymbol = SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol.IsExtensionMethod && !methodSymbol.IsStatic)
                argumentIndex++;

            SyntaxReference declaringMethodReference = methodSymbol?.DeclaringSyntaxReferences.FirstOrDefault();
            if (declaringMethodReference == null) { // Method chain stops here -> Resolve invocation
                await ResolveNode(invocation);
                return;
            }

            if (!((await declaringMethodReference.GetSyntaxAsync()) is MethodDeclarationSyntax methodDeclaration))
                return;

            // Check indexing
            if (methodDeclaration.ParameterList.Parameters.Count < argumentIndex)
                return;

            ParameterSyntax parameter = methodDeclaration.ParameterList.Parameters[argumentIndex];

            // parameter could be out of semanticModel => Get analyser for the right semantic model
            DiscordContextAnalyser contextAnalyser = HasSameSemantics(parameter) ? this : GetNewAnalyser<DiscordContextAnalyser>(parameter);

            IParameterSymbol parameterSymbol = contextAnalyser.SemanticModel.GetDeclaredSymbol(parameter);

            foreach (SyntaxNode node in await GetReferences(parameterSymbol)) {
                if (FoundContext)
                    return;

                await contextAnalyser.ResolveNode(node);
            }

            if (contextAnalyser.context != DiscordBaseCompletionContext.Undefined) {
                this.context = contextAnalyser.context;
                this.channelContext = contextAnalyser.channelContext;
            }
        }

        private void CheckForContext(ExpressionSyntax expression) {
            INamedTypeSymbol contextTypeSymbol = SemanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;
            if (contextTypeSymbol == null)
                return;

            if (contextTypeSymbol.Name.Contains("Task")) // Get first type argument of Task / ValueTask
                contextTypeSymbol = contextTypeSymbol.TypeArguments[0] as INamedTypeSymbol;

            // Set context with found interface
            if (DiscordNameCollection.Contains(contextTypeSymbol.ToDisplayString())) {
                SetContext(contextTypeSymbol);
                this.contextNode = expression;
                return;
            }

            // If interface not found check inheritance 
            IEnumerable<INamedTypeSymbol> foundInterfaces = contextTypeSymbol.AllInterfaces
                .Where(e => DiscordNameCollection.Contains(e.ToDisplayString()));

            foreach (INamedTypeSymbol interfaceSymbol in foundInterfaces) {
                SetContext(interfaceSymbol);
                if (context != DiscordBaseCompletionContext.Undefined) {
                    this.contextNode = expression;
                    if (context == DiscordBaseCompletionContext.Channel)
                        HandleChannelType(contextTypeSymbol);

                    return;
                }
            }
        }

        private void SetContext(ITypeSymbol interfaceSymbol) {
            switch (interfaceSymbol.ToDisplayString()) {
                case DiscordNameCollection.IGUILD:
                    context = DiscordBaseCompletionContext.Server;
                    break;
                case DiscordNameCollection.IUSER:
                    context = DiscordBaseCompletionContext.User;
                    break;
                case DiscordNameCollection.IROLE:
                    context = DiscordBaseCompletionContext.Role;
                    break;
                case DiscordNameCollection.ICHANNEL:
                    context = DiscordBaseCompletionContext.Channel;
                    break;
            }
        }

        private void HandleChannelType(INamedTypeSymbol contextTypeSymbol) {
            if (DiscordNameCollection.ChannelContains(contextTypeSymbol.ToDisplayString())) {
                channelContext = ResolveChannelType(contextTypeSymbol.ToDisplayString());
                return;
            }

            IEnumerable<string> foundNames = contextTypeSymbol.AllInterfaces
                .Select(e => e.ToDisplayString())
                .Where(e => DiscordNameCollection.ChannelContains(e));

            DiscordChannelContext? temp = ResolveChannelType(foundNames);

            if (temp.HasValue) {
                channelContext = temp;
                return;
            }
        }

        private DiscordChannelContext? ResolveChannelType(string name) {
            switch (name) {
                case DiscordNameCollection.ITEXTCHANNEL:
                    return DiscordChannelContext.Text;
                case DiscordNameCollection.IVOICECHANNEL:
                    return DiscordChannelContext.Voice;
                case DiscordNameCollection.ICATEGORYCHANNEL:
                    return DiscordChannelContext.Category;
                case DiscordNameCollection.IDMCHANNEL:
                    return DiscordChannelContext.DM;
                case DiscordNameCollection.IFORUMCHANNEL:
                    return DiscordChannelContext.Forum;
                case DiscordNameCollection.IGUILDCHANNEL:
                    return DiscordChannelContext.Guild;
                case DiscordNameCollection.IGROUPCHANNEL:
                    return DiscordChannelContext.Group;
                case DiscordNameCollection.ISTAGECHANNEL:
                    return DiscordChannelContext.Stage;
                case DiscordNameCollection.ITHREADCHANNEL:
                    return DiscordChannelContext.Thread;
                case DiscordNameCollection.IPRIVATECHANNEL:
                    return DiscordChannelContext.Private;
            }
            return null;
        }

        private DiscordChannelContext? ResolveChannelType(IEnumerable<string> allInterfaces) {
            // Order is important
            // Some Interfaces inherit from same --> Less specific Interfaces first
            if (allInterfaces.Any(e => e == DiscordNameCollection.ISTAGECHANNEL))
                return DiscordChannelContext.Stage;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IDMCHANNEL))
                return DiscordChannelContext.DM;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IPRIVATECHANNEL))
                return DiscordChannelContext.Private;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IVOICECHANNEL))
                return DiscordChannelContext.Voice;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IFORUMCHANNEL))
                return DiscordChannelContext.Forum;

            if (allInterfaces.Any(e => e == DiscordNameCollection.ICATEGORYCHANNEL))
                return DiscordChannelContext.Category;

            if (allInterfaces.Any(e => e == DiscordNameCollection.ITHREADCHANNEL))
                return DiscordChannelContext.Thread;

            if (allInterfaces.Any(e => e == DiscordNameCollection.ITEXTCHANNEL))
                return DiscordChannelContext.Text;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IGROUPCHANNEL))
                return DiscordChannelContext.Group;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IGUILDCHANNEL))
                return DiscordChannelContext.Guild;


            return null;
        }
        #endregion

        #region Helper
        private bool FoundContext => this.context != DiscordBaseCompletionContext.Undefined;
        private static int GetArgumentIndex(ArgumentSyntax argument) => ((ArgumentListSyntax)argument.Parent).Arguments.IndexOf(argument);
        #endregion
    }
}
