using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using HellBrick.Refactorings.Utils;

namespace HellBrick.Refactorings.ExpressionBodies
{
	public abstract class BaseMethodExpressionBodyHandler<TDeclaration> : IExpressionBodyHandler<TDeclaration>
		where TDeclaration : BaseMethodDeclarationSyntax
	{
		public bool CanConvertToExpression( TDeclaration declaration ) => true;
		public BlockSyntax GetBody( TDeclaration declaration ) => declaration.Body;
		public SyntaxNode GetRemovedNode( TDeclaration declaration ) => declaration.Body;

		public abstract string GetIdentifierName( TDeclaration declaration );
		public abstract TDeclaration ReplaceBodyWithExpressionClause( TDeclaration declaration, ArrowExpressionClauseSyntax arrow );
	}

	public class MethodExpressionBodyHandler : BaseMethodExpressionBodyHandler<MethodDeclarationSyntax>
	{
		public override string GetIdentifierName( MethodDeclarationSyntax declaration ) => declaration.Identifier.Text;

		public override MethodDeclarationSyntax ReplaceBodyWithExpressionClause( MethodDeclarationSyntax declaration, ArrowExpressionClauseSyntax arrow ) =>
			declaration
				.WithBody( null )
				.WithExpressionBody( arrow )
				.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
	}

	public class OperatorExpressionBodyHandler : BaseMethodExpressionBodyHandler<OperatorDeclarationSyntax>
	{
		public override string GetIdentifierName( OperatorDeclarationSyntax declaration ) => "operator " + declaration.OperatorToken.ToString();

		public override OperatorDeclarationSyntax ReplaceBodyWithExpressionClause( OperatorDeclarationSyntax declaration, ArrowExpressionClauseSyntax arrow ) =>
			declaration
				.WithBody( null )
				.WithExpressionBody( arrow )
				.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
	}

	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodiedMethodRefactoring ) ), Shared]
	public class ToExpressionBodiedMethodRefactoring : AbstractExpressionBodyRefactoring<MethodDeclarationSyntax>
	{
		public ToExpressionBodiedMethodRefactoring() : base( new MethodExpressionBodyHandler() )
		{
		}
	}

	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodiedMethodRefactoring ) ), Shared]
	public class ToExpressionBodiedOperatorRefactoring : AbstractExpressionBodyRefactoring<OperatorDeclarationSyntax>
	{
		public ToExpressionBodiedOperatorRefactoring() : base( new OperatorExpressionBodyHandler() )
		{
		}
	}
}