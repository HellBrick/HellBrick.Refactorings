using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HellBrick.Refactorings.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HellBrick.Refactorings.ExpressionBodies
{
	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodyRefactoring ) ), Shared]
	public class ToBlockBodyRefactoring : CodeRefactoringProvider
	{
		private readonly CodeRefactoringProvider[] _subProviders = new CodeRefactoringProvider[]
		{
			new DelcarationSpecificToBlockBodyRefactoring<MethodDeclarationSyntax>( new MethodExpressionBodyHandler() ),
			new DelcarationSpecificToBlockBodyRefactoring<OperatorDeclarationSyntax>( new OperatorExpressionBodyHandler() ),
			new DelcarationSpecificToBlockBodyRefactoring<PropertyDeclarationSyntax>( new PropertyExpressionBodyHandler() )
		};

		public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
		{
			foreach ( CodeRefactoringProvider subProvider in _subProviders )
				await subProvider.ComputeRefactoringsAsync( context ).ConfigureAwait( false );
		}

		private class DelcarationSpecificToBlockBodyRefactoring<TDeclaration> : CodeRefactoringProvider
			where TDeclaration : MemberDeclarationSyntax
		{
			private readonly IExpressionBodyHandler<TDeclaration> _handler;

			public DelcarationSpecificToBlockBodyRefactoring( IExpressionBodyHandler<TDeclaration> handler )
			{
				_handler = handler;
			}

			public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
			{
				SyntaxNode root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false );
				SemanticModel semanticModel = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false );

				IEnumerable<ArrowDeclaration> arrowDeclarations =
					from member in root.EnumerateSelectedNodes<TDeclaration>( context.Span )
					let arrow = _handler.GetArrow( member )
					where arrow != null
					select new ArrowDeclaration( member, arrow );

				foreach ( ArrowDeclaration arrowDeclaration in arrowDeclarations )
				{
					string memberName = _handler.GetMemberName( arrowDeclaration.Declaration, semanticModel );
					CodeAction codeFix = CodeAction.Create( $"Convert '{memberName}' to a block-bodied member", c => ConvertToBlockBodiedMemberAsync( arrowDeclaration, context, root, c ) );
					context.RegisterRefactoring( codeFix );
				}
			}

			private Task<Document> ConvertToBlockBodiedMemberAsync( ArrowDeclaration arrowDeclaration, CodeRefactoringContext context, SyntaxNode root, CancellationToken ct )
			{
				TDeclaration newMember = BuildNewMember( arrowDeclaration );
				Document newDocument = context.Document.WithSyntaxRoot( root.ReplaceNode( arrowDeclaration.Declaration, newMember ) );
				return Task.FromResult( newDocument );
			}

			private TDeclaration BuildNewMember( ArrowDeclaration arrowDeclaration )
			{
				StatementSyntax statement = _handler.CreateStatement( arrowDeclaration.Arrow.Expression, arrowDeclaration.Declaration );
				BlockSyntax body = Block( statement ).WithLeadingTrivia( SyntaxTrivia( SyntaxKind.EndOfLineTrivia, Environment.NewLine ) );
				TDeclaration newMember = _handler.ReplaceExpressionClauseWithBody( arrowDeclaration.Declaration, body );
				return newMember;
			}

			private struct ArrowDeclaration : IEquatable<ArrowDeclaration>
			{
				public ArrowDeclaration( TDeclaration declaration, ArrowExpressionClauseSyntax arrow )
				{
					Declaration = declaration;
					Arrow = arrow;
				}

				public TDeclaration Declaration { get; }
				public ArrowExpressionClauseSyntax Arrow { get; }

				public override int GetHashCode()
				{
					unchecked
					{
						const int prime = -1521134295;
						int hash = 12345701;
						hash = hash * prime + EqualityComparer<TDeclaration>.Default.GetHashCode( Declaration );
						hash = hash * prime + EqualityComparer<ArrowExpressionClauseSyntax>.Default.GetHashCode( Arrow );
						return hash;
					}
				}

				public bool Equals( ArrowDeclaration other ) => EqualityComparer<TDeclaration>.Default.Equals( Declaration, other.Declaration ) && EqualityComparer<ArrowExpressionClauseSyntax>.Default.Equals( Arrow, other.Arrow );
				public override bool Equals( object obj ) => obj is ArrowDeclaration && Equals( (ArrowDeclaration) obj );

				public static bool operator ==( ArrowDeclaration x, ArrowDeclaration y ) => x.Equals( y );
				public static bool operator !=( ArrowDeclaration x, ArrowDeclaration y ) => !x.Equals( y );
			}
		}
	}
}
