using Discord.NET.SupportExtension.Core.Interface.Analyser;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class DiscordServerIdAnalyser : DiscordAnalyserBase, IDiscordServerIdAnalyser {
        private readonly ILogger<DiscordServerIdAnalyser> logger;
        private const int RECURSIONLEVEL = 4;
        private List<ulong> serverIds = new List<ulong>();

        private SyntaxNode currentNode;

        public DiscordServerIdAnalyser() {
            logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<DiscordServerIdAnalyser>();
        }

        public async Task<ImmutableArray<ulong>> Run(SyntaxNode node) {
            currentNode = node;

            for (int i = 0; i < RECURSIONLEVEL && !(currentNode is BlockSyntax) && currentNode != null; i++) {
                if (currentNode is ExpressionSyntax)
                    await ResolveNodeAsync(currentNode);

                if (serverIds.Any())
                    return serverIds.ToImmutableArray();

                currentNode = currentNode.Parent;
            }

            return serverIds.ToImmutableArray();
        }


        public async Task ResolveNodeAsync(SyntaxNode node) {
            switch (node) {
                case IdentifierNameSyntax identifier:
                    await HandleIdentifierAsync(identifier);
                    break;
                case BinaryExpressionSyntax binary:
                    await ResolveNodeAsync(binary.Left);
                    await ResolveNodeAsync(binary.Right);
                    break;
                case InvocationExpressionSyntax invocation:
                    await ResolveInvocationArguments(invocation.ArgumentList.Arguments);
                    if (await CheckInvocationType(invocation))
                        await ResolveNodeAsync(invocation.Expression);
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    await ResolveNodeAsync(memberAccess.Name);
                    await ResolveNodeAsync(memberAccess.Expression);
                    break;
                case AwaitExpressionSyntax awaitExpression:
                    await ResolveNodeAsync(awaitExpression.Expression);
                    break;
            }
        }

        private async Task ResolveInvocationArguments(SeparatedSyntaxList<ArgumentSyntax> arguments) {
            foreach (ArgumentSyntax argument in arguments) {
                if (argument.Expression is IdentifierNameSyntax identifier)
                    await ResolveNodeAsync(identifier);
            }
        }

        private async Task<bool> CheckInvocationType(InvocationExpressionSyntax invocation) {
            INamedTypeSymbol typeSymbol = SemanticModel.GetTypeInfo(invocation).Type as INamedTypeSymbol;

            if (typeSymbol?.Name == "Task") // Overwrite symbol with generic type
                typeSymbol = typeSymbol.TypeArguments[0] as INamedTypeSymbol;

            if (typeSymbol == null)
                return true;

            if (typeSymbol.ToDisplayString() != DiscordNameCollection.IGUILD && typeSymbol.AllInterfaces.All(e => e.ToDisplayString() != DiscordNameCollection.IGUILD))
                return true;

            LiteralExpressionSyntax numericLiteral = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax;
            if (numericLiteral == null || !numericLiteral.IsKind(SyntaxKind.NumericLiteralExpression))
                return true;

            ulong guildId;
            try {
                guildId = Convert.ToUInt64(numericLiteral.Token.Value);
            }
            catch {
                logger.LogError($"Cannot cast {numericLiteral.Token.Value} to a valid guild id.");
                return true;
            }

            serverIds.Add(guildId);
            return false;
        }

        private async Task HandleIdentifierAsync(IdentifierNameSyntax identifier) {
            ISymbol identifierSymbol = SemanticModel.GetSymbolInfo(identifier).Symbol;

            if (identifierSymbol == null)
                return;

            SyntaxReference foundDefinition = identifierSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (foundDefinition == null)
                return;

            SyntaxNode foundDefinitionSyntax = await foundDefinition.GetSyntaxAsync();

            IEnumerable<ReferencedSymbol> references = await SymbolFinder.FindReferencesAsync(identifierSymbol, Solution, Documents);
            IEnumerable<Location> referenceLocations = references.SelectMany(l => l.Locations.Select(s => s.Location));

            foreach (var location in referenceLocations) {
                SyntaxNode referencedNode = (await location.SourceTree.GetRootAsync()).FindNode(location.SourceSpan);

                if (referencedNode.FullSpan == identifier.FullSpan)
                    continue;

                DiscordServerIdAnalyser serverIdAnalyser;
                if (referencedNode.SyntaxTree.FilePath != SyntaxTree.FilePath) {
                    SemanticModel sm = SemanticModel.Compilation.GetSemanticModel(referencedNode.SyntaxTree);
                    serverIdAnalyser = new DiscordServerIdAnalyser();
                    serverIdAnalyser.Initialize(Solution, Project, SemanticModel);
                    serverIdAnalyser.serverIds = this.serverIds;
                }
                else
                    serverIdAnalyser = this;

                if (referencedNode.Parent is AssignmentExpressionSyntax assignment)
                    await serverIdAnalyser.ResolveNodeAsync(assignment.Right);

                if (referencedNode.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is AssignmentExpressionSyntax parentAssignment)
                    await serverIdAnalyser.ResolveNodeAsync(parentAssignment);

                if (foundDefinitionSyntax is ClassDeclarationSyntax) {
                    if (referencedNode.Parent is VariableDeclarationSyntax variableDeclaration) {
                        ObjectCreationExpressionSyntax objectCreation = variableDeclaration.Variables[0].Initializer?.Value as ObjectCreationExpressionSyntax;
                        foreach (ArgumentSyntax argumentSyntax in objectCreation?.ArgumentList.Arguments)
                            await serverIdAnalyser.ResolveNodeAsync(argumentSyntax.Expression);
                    }
                }
            }

            switch (foundDefinitionSyntax) {
                case ParameterSyntax parameter:
                    await HandleIncomingParameterAsync(parameter);
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    await ResolveNodeAsync(propertyDeclaration.Initializer?.Value);
                    break;
                case VariableDeclaratorSyntax variableDeclarator:
                    await ResolveNodeAsync(variableDeclarator.Initializer?.Value);
                    break;
            }
        }

        private async Task HandleIncomingParameterAsync(ParameterSyntax parameter) {
            ISymbol declarationSymbol = SemanticModel.GetDeclaredSymbol(parameter.Parent.Parent);

            if (declarationSymbol == null && parameter.Parent is LambdaExpressionSyntax lambda)
                await HandleLambdaParameter(lambda);

            if (declarationSymbol == null)
                return;

            IEnumerable<SymbolCallerInfo> callerInfo = await SymbolFinder.FindCallersAsync(declarationSymbol, Solution, Documents);
            IEnumerable<Location> callerLocations = callerInfo.SelectMany(e => e.Locations);
            List<Location> distinctLocations = new List<Location>();

            // Identifiers will be resolved in new analyser --> Only start 1 new analyser for all identifiers found in file
            foreach (Location location in callerLocations) {
                SyntaxNode locationNode = (await location.SourceTree.GetRootAsync()).FindNode(location.SourceSpan);
                if (locationNode is IdentifierNameSyntax) {
                    if (distinctLocations.Any(e => e.SourceTree.FilePath == location.SourceTree.FilePath))
                        continue;
                }

                distinctLocations.Add(location);
            }

            foreach (var location in distinctLocations) {
                SyntaxNode locationNode = (await location.SourceTree.GetRootAsync()).FindNode(location.SourceSpan);
                IDiscordServerIdAnalyser serverIdAnalyser;
                if (parameter.SyntaxTree.FilePath != location.SourceTree.FilePath) {

                    SyntaxTree syntaxTree = locationNode.SyntaxTree;
                    SemanticModel semanticModel = this.SemanticModel.Compilation.GetSemanticModel(syntaxTree);

                    serverIdAnalyser = DIContainer.GetService<IDiscordServerIdAnalyser>();
                    serverIdAnalyser.Initialize(Solution, Project, semanticModel);
                }
                else
                    serverIdAnalyser = this;

                ImmutableArray<ulong> temp = await serverIdAnalyser.Run(locationNode);
                foreach (ulong value in temp) {
                    if (!serverIds.Contains(value))
                        serverIds.Add(value);
                }
            }
        }

        private async Task HandleLambdaParameter(LambdaExpressionSyntax lambda) {
            SyntaxNode temp = lambda.Parent;

            while (temp != null && !temp.IsKind(SyntaxKind.Block)) {
                if (temp is InvocationExpressionSyntax invocation) {
                    await ResolveNodeAsync(invocation);
                    break;
                }

                temp = temp.Parent;
            }
        }
    }
}