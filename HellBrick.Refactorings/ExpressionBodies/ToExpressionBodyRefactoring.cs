using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using HellBrick.Refactorings.Utils;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Composition;

namespace HellBrick.Refactorings.ExpressionBodies
{
	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodyRefactoring ) ), Shared]
	public class ToExpressionBodyRefactoring : CodeRefactoringProvider
	{
		private CodeRefactoringProvider[] _subProviders = new CodeRefactoringProvider[]
		{
			new ToExpressionBodyRefactoring<MethodDeclarationSyntax>( new MethodExpressionBodyHandler() ),
			new ToExpressionBodyRefactoring<OperatorDeclarationSyntax>( new OperatorExpressionBodyHandler() ),
			new ToExpressionBodyRefactoring<PropertyDeclarationSyntax>( new PropertyExpressionBodyHandler() )
		};

		public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
		{
			foreach ( CodeRefactoringProvider subProvider in _subProviders )
				await subProvider.ComputeRefactoringsAsync( context ).ConfigureAwait( false );
		}
	}

	public class ToExpressionBodyRefactoring<TDeclarationSyntax> : CodeRefactoringProvider
		where TDeclarationSyntax : MemberDeclarationSyntax
	{
		private readonly IExpressionBodyHandler<TDeclarationSyntax> _handler;

		public ToExpressionBodyRefactoring( IExpressionBodyHandler<TDeclarationSyntax> handler )
		{
			_handler = handler;
		}

		public async sealed override Task ComputeRefactoringsAsync( CodeRefactoringContext context )
		{
			SyntaxNode root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false );
			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false );

			IEnumerable<OneLiner> oneLiners = EnumerateOneLiners( context, root );

			foreach ( OneLiner oneLiner in oneLiners )
			{
				string memberName = semanticModel.GetDeclaredSymbol( oneLiner.Declaration, context.CancellationToken )?.Name ?? _handler.GetIdentifierName( oneLiner.Declaration );
				CodeAction codeFix = CodeAction.Create( $"Convert '{memberName}' to an expression-bodied member", c => ConvertToExpressionBodiedMemberAsync( oneLiner, context, root, c ) );
				context.RegisterRefactoring( codeFix );
			}
		}

		private IEnumerable<OneLiner> EnumerateOneLiners( CodeRefactoringContext context, SyntaxNode root ) =>
			from TDeclarationSyntax declaration in root.EnumerateSelectedNodes<TDeclarationSyntax>( context.Span )
			where _handler.CanConvertToExpression( declaration )
			let body = _handler.GetBody( declaration )
			where body?.Statements.Count == 1
			let expression = TryGetLambdableExpression( body.Statements[ 0 ] )
			where expression != null
			select new OneLiner( declaration, expression );

		private static ExpressionSyntax TryGetLambdableExpression( StatementSyntax singleStatement )
			=> ( singleStatement as ReturnStatementSyntax )?.Expression
			?? ( singleStatement as ExpressionStatementSyntax )?.Expression;

		private Task<Document> ConvertToExpressionBodiedMemberAsync( OneLiner oneLiner, CodeRefactoringContext context, SyntaxNode root, CancellationToken cancellationToken )
		{
			TDeclarationSyntax newMember = BuildNewMember( oneLiner );
			var newDocument = context.Document.WithSyntaxRoot( root.ReplaceNode( oneLiner.Declaration, newMember ) );
			return Task.FromResult( newDocument );
		}

		private TDeclarationSyntax BuildNewMember( OneLiner oneLiner )
		{
			TDeclarationSyntax newMember = oneLiner.Declaration;

			//	Remove the \r\n if it's the only trailing trivia
			SyntaxNode removedNode = _handler.GetRemovedNode( oneLiner.Declaration );
			SyntaxNode lastMaintainedNode = oneLiner.Declaration.FindNode( new TextSpan( removedNode.FullSpan.Start - 1, 0 ) );
			SyntaxTriviaList lastMaintainedNodeTrivia = lastMaintainedNode.GetTrailingTrivia();
			if ( lastMaintainedNodeTrivia.Count == 1 && lastMaintainedNodeTrivia[ 0 ].IsKind( SyntaxKind.EndOfLineTrivia ) )
			{
				newMember = newMember.ReplaceNode(
					removedNode,
					removedNode.ReplaceTrivia( lastMaintainedNodeTrivia[ 0 ], SyntaxTrivia( SyntaxKind.WhitespaceTrivia, " " ) ) );
			}

			ExpressionSyntax returnExpression = oneLiner.Expression.WithLeadingTrivia( SyntaxTrivia( SyntaxKind.WhitespaceTrivia, " " ) );
			ArrowExpressionClauseSyntax arrow = ArrowExpressionClause( returnExpression );

			return _handler.ReplaceBodyWithExpressionClause( newMember, arrow );
		}

		private class OneLiner
		{
			public OneLiner( TDeclarationSyntax declaration, ExpressionSyntax expression )
			{
				Declaration = declaration;
				Expression = expression;
			}

			public TDeclarationSyntax Declaration { get; }
			public ExpressionSyntax Expression { get; }
		}
	}
}
