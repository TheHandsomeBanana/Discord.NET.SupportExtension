using Discord.NET.SupportExtension.Core.Interface.Analyser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class DiscordServerIdAnalyser : DiscordAnalyserBase, IDiscordServerIdAnalyser {
        private HashSet<FileLinePositionSpan> visited = new HashSet<FileLinePositionSpan>();
        private List<ulong> serverIds = new List<ulong>();

        public async Task<ImmutableArray<ulong>> Run(SyntaxNode node) {
            if (InitiateAnalysis(node))
                await ResolveNodeAsync(node);

            return serverIds.ToImmutableArray();
        }

        private bool InitiateAnalysis(SyntaxNode contextNode) {
            return contextNode is InvocationExpressionSyntax || contextNode is IdentifierNameSyntax;
        }

        public async Task ResolveNodeAsync(SyntaxNode node) {
            if (serverIds.Count > 0 || node is null)
                return;

            FileLinePositionSpan nodeSpan = node.GetLocation().GetLineSpan();
            if (visited.Contains(nodeSpan))
                return;

            visited.Add(nodeSpan);

            switch (node) {
                case IdentifierNameSyntax identifier:
                    await HandleIdentifierAsync(identifier);
                    break;
                case BinaryExpressionSyntax binary:
                    await ResolveNodeAsync(binary.Left);
                    await ResolveNodeAsync(binary.Right);
                    break;
                case InvocationExpressionSyntax invocation:
                    bool success = false;
                    if (IsGuildType(invocation)) {
                        success = AddServerId(invocation);
                        if (!success)
                            await CheckMethodImplementation(invocation);
                    }

                    if (!success)
                        await ResolveNodeAsync(invocation.Expression);

                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    if (memberAccess.Name is IdentifierNameSyntax memberIdentifier && IsGuildType(memberIdentifier))
                        await ResolveNodeAsync(memberAccess.Name);

                    await ResolveNodeAsync(memberAccess.Expression);
                    break;
                case AwaitExpressionSyntax awaitExpression:
                    await ResolveNodeAsync(awaitExpression.Expression);
                    break;
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    await ResolveNodeAsync(parenthesizedExpression.Expression);
                    break;
            }
        }



        private async Task CheckMethodImplementation(InvocationExpressionSyntax invocation) {
            if (!(SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol))
                return;

            SyntaxReference foundMethod = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (foundMethod == null)
                return;

            SyntaxNode foundMethodSyntax = await foundMethod.GetSyntaxAsync();
            if (!(foundMethodSyntax is MethodDeclarationSyntax methodDeclaration))
                return;

            DiscordServerIdAnalyser analyser = HasSameSemantics(methodDeclaration) ? this : CreateNew(methodDeclaration);
            foreach (InvocationExpressionSyntax childInvocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
                await analyser.ResolveNodeAsync(childInvocation);
        }

        private async Task HandleIdentifierAsync(IdentifierNameSyntax identifier) {
            ISymbol identifierSymbol = SemanticModel.GetSymbolInfo(identifier).Symbol;

            if (identifierSymbol == null)
                return;

            SyntaxReference foundIdentifier = identifierSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (foundIdentifier == null)
                return;

            SyntaxNode foundIdentifierSyntax = await foundIdentifier.GetSyntaxAsync();

            foreach (SyntaxNode node in await GetReferences(identifierSymbol)) {
                DiscordServerIdAnalyser analyser = HasSameSemantics(node) ? this : CreateNew(node);

                if (node.Parent is AssignmentExpressionSyntax assignment)
                    await analyser.ResolveNodeAsync(assignment.Right);

                if (node.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Parent is AssignmentExpressionSyntax parentAssignment)
                    await analyser.ResolveNodeAsync(parentAssignment);

                if (foundIdentifierSyntax is ClassDeclarationSyntax) {
                    if (node.Parent is VariableDeclarationSyntax variableDeclaration) {
                        ObjectCreationExpressionSyntax objectCreation = variableDeclaration.Variables[0].Initializer?.Value as ObjectCreationExpressionSyntax;
                        foreach (ArgumentSyntax argumentSyntax in objectCreation?.ArgumentList.Arguments)
                            await analyser.ResolveNodeAsync(argumentSyntax.Expression);
                    }
                }
            }

            DiscordServerIdAnalyser serverIdAnalyser = HasSameSemantics(foundIdentifierSyntax) ? this : CreateNew(foundIdentifierSyntax);

            switch (foundIdentifierSyntax) {
                case ParameterSyntax parameter:
                    await serverIdAnalyser.HandleIncomingParameterAsync(parameter);
                    break;
                case PropertyDeclarationSyntax propertyDeclaration:
                    await serverIdAnalyser.ResolvePropertyDeclaration(propertyDeclaration);
                    break;
                case VariableDeclaratorSyntax variableDeclarator:
                    await serverIdAnalyser.ResolveNodeAsync(variableDeclarator.Initializer?.Value);
                    break;
            }
        }

        private async Task ResolvePropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration) {
            if (propertyDeclaration.ExpressionBody is ArrowExpressionClauseSyntax arrowExpression) {
                await ResolveNodeAsync(arrowExpression.Expression);
                return;
            }

            await ResolveNodeAsync(propertyDeclaration.Initializer?.Value);

            AccessorDeclarationSyntax getAccessor = GetAccessor(propertyDeclaration, SyntaxKind.GetAccessorDeclaration);
            await ResolvePropertyAccessor(getAccessor);

            AccessorDeclarationSyntax setAccessor = GetAccessor(propertyDeclaration, SyntaxKind.SetAccessorDeclaration);
            await ResolvePropertyAccessor(setAccessor);
        }

        private AccessorDeclarationSyntax GetAccessor(PropertyDeclarationSyntax propertyDeclaration, SyntaxKind accessorKind)
            => propertyDeclaration.AccessorList?.Accessors.FirstOrDefault(e => e.IsKind(accessorKind));
        private async Task ResolvePropertyAccessor(AccessorDeclarationSyntax accessorDeclaration) {
            if (accessorDeclaration != null) {
                if (accessorDeclaration.ExpressionBody != null)
                    await ResolveNodeAsync(accessorDeclaration.ExpressionBody.Expression);
                else {
                    IEnumerable<SyntaxNode> children = accessorDeclaration.Body?.DescendantNodes() ?? Array.Empty<SyntaxNode>();
                    foreach (SyntaxNode child in children)
                        await ResolveNodeAsync(child);
                }
            }
        }

        private async Task HandleIncomingParameterAsync(ParameterSyntax parameter) {
            ISymbol methodSymbol = SemanticModel.GetDeclaredSymbol(parameter.Parent.Parent);

            if (methodSymbol == null && parameter.Parent is LambdaExpressionSyntax lambda)
                await HandleLambdaParameter(lambda);

            if (methodSymbol == null)
                return;

            foreach (SyntaxNode node in await GetCallers(methodSymbol)) {
                DiscordServerIdAnalyser analyser = HasSameSemantics(node) ? this : CreateNew(node);

                await analyser.ResolveNodeAsync(node);
                MapFromAnalyser(analyser);
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

        private bool IsGuildType(INamedTypeSymbol typeSymbol) => typeSymbol.ToDisplayString() == DiscordNameCollection.IGUILD
            || typeSymbol.AllInterfaces.Any(e => e.ToDisplayString() == DiscordNameCollection.IGUILD);
        private bool IsGuildType(InvocationExpressionSyntax invocation) {
            INamedTypeSymbol typeSymbol = SemanticModel.GetTypeInfo(invocation).Type as INamedTypeSymbol;

            if (typeSymbol?.Name == "Task") // Overwrite symbol with generic type
                typeSymbol = typeSymbol.TypeArguments[0] as INamedTypeSymbol;

            if (typeSymbol == null)
                return false;

            return IsGuildType(typeSymbol);
        }
        private bool IsGuildType(IdentifierNameSyntax identifier) {
            INamedTypeSymbol typeSymbol = SemanticModel.GetTypeInfo(identifier).Type as INamedTypeSymbol;
            if (typeSymbol?.Name == "Task") // Overwrite symbol with generic type
                typeSymbol = typeSymbol.TypeArguments[0] as INamedTypeSymbol;

            if (typeSymbol == null)
                return false;

            return IsGuildType(typeSymbol);
        }
        private bool AddServerId(InvocationExpressionSyntax invocation) {
            ExpressionSyntax expression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (expression is null)
                return false;

            ulong? guildId = null;

            if (expression is SimpleLambdaExpressionSyntax simpleLambdaExpression && simpleLambdaExpression.ExpressionBody is BinaryExpressionSyntax binaryExpression)
                expression = binaryExpression.Right; // Right expression can be numeric literal .FirstOrDefault(e => e.Id == 0002);

            if (expression is LiteralExpressionSyntax numericLiteral && numericLiteral.IsKind(SyntaxKind.NumericLiteralExpression)) {
                try {
                    guildId = Convert.ToUInt64(numericLiteral.Token.Value);
                }
                catch {
                    return false;
                }
            }

            if (guildId.HasValue) {
                serverIds.Add(guildId.Value);
                return true;
            }

            return false;
        }

        private DiscordServerIdAnalyser CreateNew(SyntaxNode node) {
            DiscordServerIdAnalyser analyser = GetNewAnalyser<DiscordServerIdAnalyser>(node);
            MapToAnalyser(analyser);
            return analyser;
        }
        private void MapFromAnalyser(DiscordServerIdAnalyser analyser) {
            this.serverIds = analyser.serverIds;
            this.visited = analyser.visited;
        }
        private void MapToAnalyser(DiscordServerIdAnalyser analyser) {
            analyser.serverIds = this.serverIds;
            analyser.visited = this.visited;
        }
    }
}