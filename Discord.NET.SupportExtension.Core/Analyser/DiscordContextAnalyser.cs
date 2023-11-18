using Discord.NET.SupportExtension.Core.Interface;
using Discord.WebSocket;
using HB.NETF.Code.Analysis.Analyser;
using HB.NETF.Code.Analysis.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class DiscordContextAnalyser : DiscordAnalyserBase, ICodeAnalyser<DiscordCompletionContext> {
        private DiscordBaseCompletionContext context;
        private DiscordChannelContext? channelContext;
        private bool hasBaseContextContext;
        private SyntaxNode currentNode;


        public async Task<DiscordCompletionContext> Run(SyntaxNode node) {
            currentNode = node;
            if (currentNode.IsKind(SyntaxKind.NumericLiteralExpression))
                currentNode = currentNode.Parent;

            if (await InitiateDetection(currentNode))
                await ResolveNode(currentNode);

            if (hasBaseContextContext)
                return new DiscordCompletionContext(context, channelContext);

            return new DiscordCompletionContext();
        }


        #region Initiate Analysis
        private async Task<bool> InitiateDetection(SyntaxNode trigger) {
            if (trigger is ExpressionSyntax)
                return true;

            if (trigger is ArgumentListSyntax || trigger is ArgumentSyntax)
                return await CheckMethodArgumentUsage(trigger);

            return trigger is ExpressionSyntax || trigger is ArgumentListSyntax || trigger is ArgumentSyntax;
        }

        private static readonly string[] ulongTypes = { "ulong", nameof(UInt64) };
        private async Task<bool> CheckMethodArgumentUsage(SyntaxNode trigger) {
            int argumentIndex = -1;
            InvocationExpressionSyntax parentInvocation;
            switch (trigger) {
                case ArgumentSyntax argument:
                    if (!argument.Expression.IsKind(SyntaxKind.NumericLiteralExpression))
                        return false;

                    argumentIndex = ((ArgumentListSyntax)argument.Parent).Arguments.IndexOf(argument);
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

            if (argumentIndex == -1)
                return false;

            IMethodSymbol methodSymbol = SemanticModel.GetSymbolInfo(parentInvocation).Symbol as IMethodSymbol;
            SyntaxReference methodSyntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (methodSyntaxReference == null) // Basic Methods from Discord are out of assembly
                return true;

            MethodDeclarationSyntax methodDeclaration = (await methodSyntaxReference.GetSyntaxAsync()) as MethodDeclarationSyntax;
            ParameterSyntax parameterFromArgumentIndex = methodDeclaration.ParameterList.Parameters[argumentIndex];
            if (!ulongTypes.Contains(parameterFromArgumentIndex.Type.ToString())) // Only resolve param with ulong type
                return false;


            IEnumerable<ReferencedSymbol> parameterReferenceSymbols = await SymbolFinder.FindReferencesAsync(SemanticModel.GetDeclaredSymbol(methodDeclaration), Solution, Documents);
            


            return false;
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
                    await ResolveNode(argument.Parent);
                    break;
                case ArgumentListSyntax argumentList:
                    await ResolveNode(argumentList.Parent);
                    break;

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
                hasBaseContextContext = true;
                return;
            }

            // If interface not found check inheritance 
            foreach (INamedTypeSymbol interfaceSymbol in contextTypeSymbol.AllInterfaces) {
                SetContext(interfaceSymbol);
                if (context != DiscordBaseCompletionContext.Undefined) {
                    if (context == DiscordBaseCompletionContext.Channel)
                        HandleChannelType(contextTypeSymbol);

                    hasBaseContextContext = true;
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

            DiscordChannelContext? temp = ResolveChannelType(contextTypeSymbol.AllInterfaces.Select(e => e.ToDisplayString()));

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
    }
}
