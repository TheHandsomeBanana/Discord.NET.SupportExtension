using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.ContextDetector {
    internal class AsyncDiscordContextDetector : ICodeAnalyser<DiscordCompletionContext> {
        private DiscordBaseCompletionContext context;
        private DiscordChannelContext? channelContext;
        private bool hasBaseContextContext;
        private SyntaxNode currentNode;
        public SemanticModel SemanticModel { get; }


        public AsyncDiscordContextDetector(SemanticModel sm) {
            SemanticModel = sm;
        }

        public async Task<DiscordCompletionContext> Run(SyntaxNode node) {
            currentNode = node;
            if (currentNode.IsKind(SyntaxKind.NumericLiteralExpression))
                currentNode = currentNode.Parent;

            if (currentNode is ExpressionSyntax || currentNode is ArgumentListSyntax || currentNode is ArgumentSyntax)
                await ResolveNodeAsync(currentNode);

            if (hasBaseContextContext)
                return new DiscordCompletionContext(context, channelContext);

            currentNode = currentNode.Parent;

            return new DiscordCompletionContext();
        }

        async Task<object> ICodeAnalyser.Run(SyntaxNode syntaxNode) => await Run(syntaxNode);

        // Recursive node detection
        public async Task ResolveNodeAsync(SyntaxNode node) {
            switch (node) {
                case InvocationExpressionSyntax invocation:
                    CheckForContext(invocation);
                    break;
                case BinaryExpressionSyntax binary when binary.Kind() == SyntaxKind.EqualsExpression || binary.Kind() == SyntaxKind.NotEqualsExpression:
                    await ResolveNodeAsync(binary.Left);
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    await ResolveNodeAsync(memberAccess.Expression);
                    break;
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    await ResolveNodeAsync(conditionalAccess.Expression);
                    break;
                case IdentifierNameSyntax identifier:
                    CheckForContext(identifier);
                    break;
                case SwitchStatementSyntax switchStatement:
                    await ResolveNodeAsync(switchStatement.Expression);
                    break;
                case ArgumentSyntax argument:
                    await ResolveNodeAsync(argument.Parent);
                    break;
                case ArgumentListSyntax argumentList:
                    await ResolveNodeAsync(argumentList.Parent);
                    break;

            }
        }

        private void CheckForContext(ExpressionSyntax expression) {
            INamedTypeSymbol contextTypeSymbol = SemanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;

            if (contextTypeSymbol?.Name == "Task") // Overwrite symbol with generic type
                contextTypeSymbol = contextTypeSymbol.TypeArguments[0] as INamedTypeSymbol;

            if (contextTypeSymbol == null)
                return;

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
                case DiscordNameCollection.SERVERID:
                case DiscordNameCollection.SERVERIDLIST:
                case DiscordNameCollection.SERVERNAME:
                case DiscordNameCollection.SERVERNAMELIST:
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
            }

            return null;
        }

        private DiscordChannelContext? ResolveChannelType(IEnumerable<string> allInterfaces) {
            // Order is important
            // Some Interfaces inherit from same --> Less specific Interfaces first
            if (allInterfaces.Any(e => e == DiscordNameCollection.IVOICECHANNEL))
                return DiscordChannelContext.Voice;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IFORUMCHANNEL))
                return DiscordChannelContext.Forum;

            if (allInterfaces.Any(e => e == DiscordNameCollection.ICATEGORYCHANNEL))
                return DiscordChannelContext.Category;

            if (allInterfaces.Any(e => e == DiscordNameCollection.ITEXTCHANNEL))
                return DiscordChannelContext.Text;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IGROUPCHANNEL))
                return DiscordChannelContext.Group;

            if (allInterfaces.Any(e => e == DiscordNameCollection.IGUILDCHANNEL))
                return DiscordChannelContext.Guild;


            return null;
        }
    }
}
