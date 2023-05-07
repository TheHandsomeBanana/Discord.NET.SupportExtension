using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis;
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
    internal class AsyncDiscordContextDetector : IAsyncCodeAnalyser<DiscordCompletionContext> {
        private SemanticModel SemanticModel;

        private ILogger logger;

        private const int MAXRECURSION = 5;
        private DiscordCompletionContext _context;
        private bool _hasContext;
        private SyntaxNode _currNode;
        private SemanticModel _semanticModel;

        public DiscordChannelContext? ChannelType { get; private set; }

        public AsyncDiscordContextDetector(SemanticModel sm) {
            _semanticModel = sm;
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();

            logger = loggerFactory.CreateLogger(nameof(AsyncDiscordContextDetector), b => b.WithNoTargets());
        }

        public async Task<DiscordCompletionContext> ExecuteAsync(SyntaxNode node) {
            _currNode = node.Parent;
            if (_currNode.IsKind(SyntaxKind.NumericLiteralExpression))
                _currNode = _currNode.Parent;

            for (int i = 0; i < MAXRECURSION && !(_currNode is BlockSyntax) && _currNode != null; i++) {
                if (_currNode is ExpressionSyntax)
                    await ResolveNodeAsync(_currNode);

                if (_hasContext)
                    return _context;
            }

            return DiscordCompletionContext.Undefined;
        }

        // Recursive node detection
        private async Task ResolveNodeAsync(SyntaxNode node) {
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
            }
        }

        private void CheckForContext(ExpressionSyntax expression) {
            INamedTypeSymbol contextTypeSymbol = _semanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;

            if (contextTypeSymbol?.Name == "Task") // Overwrite symbol with generic type
                contextTypeSymbol = contextTypeSymbol.TypeArguments[0] as INamedTypeSymbol;

            if (contextTypeSymbol == null)
                return;

            // Set context with found interface
            if (DiscordNameCollection.Contains(contextTypeSymbol.ToDisplayString())) {
                SetContext(contextTypeSymbol);
                _hasContext = true;
                return;
            }

            // If interface not found check inheritance 
            foreach (INamedTypeSymbol interfaceSymbol in contextTypeSymbol.AllInterfaces) {
                SetContext(interfaceSymbol);
                if (_context != DiscordCompletionContext.Undefined) {
                    if (_context == DiscordCompletionContext.Channel)
                        HandleChannelType(contextTypeSymbol);

                    _hasContext = true;
                    return;
                }
            }
        }

        private void SetContext(ITypeSymbol interfaceSymbol) {
            switch (interfaceSymbol.ToDisplayString()) {
                case DiscordNameCollection.IGUILD:
                    _context = DiscordCompletionContext.Server;
                    break;
                case DiscordNameCollection.IUSER:
                    _context = DiscordCompletionContext.User;
                    break;
                case DiscordNameCollection.IROLE:
                    _context = DiscordCompletionContext.Role;
                    break;
                case DiscordNameCollection.ICHANNEL:
                    _context = DiscordCompletionContext.Channel;
                    break;
            }
        }

        private void HandleChannelType(INamedTypeSymbol contextTypeSymbol) {
            if (DiscordNameCollection.ChannelContains(contextTypeSymbol.ToDisplayString())) {
                ChannelType = ResolveChannelType(contextTypeSymbol.ToDisplayString());
                return;
            }

            DiscordChannelContext temp = ResolveChannelType(contextTypeSymbol.AllInterfaces.Select(e => e.ToDisplayString()));

            if (temp != DiscordChannelContext.Undefined) {
                ChannelType = temp;
                return;
            }
        }

        private DiscordChannelContext ResolveChannelType(string name) {
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
            }

            return DiscordChannelContext.Undefined;
        }

        private DiscordChannelContext ResolveChannelType(IEnumerable<string> allInterfaces) {
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


            return DiscordChannelContext.Undefined;
        }
    }
}
