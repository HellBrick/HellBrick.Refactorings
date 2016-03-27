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
	public abstract class ToExpressionBodiedBaseMethodRefactoring<TDeclaration> : AbstractExpressionBodyRefactoring<TDeclaration>
		where TDeclaration : BaseMethodDeclarationSyntax
	{
		protected override bool CanConvertToExpression( TDeclaration declaration ) => true;
		protected override BlockSyntax GetBody( TDeclaration declaration ) => declaration.Body;
		protected override SyntaxNode GetRemovedNode( TDeclaration declaration ) => declaration.Body;
	}

	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodiedMethodRefactoring ) ), Shared]
	public class ToExpressionBodiedMethodRefactoring : ToExpressionBodiedBaseMethodRefactoring<MethodDeclarationSyntax>
	{
		protected override string GetIdentifierName( MethodDeclarationSyntax declaration ) => declaration.Identifier.Text;

		protected override MethodDeclarationSyntax ReplaceBodyWithExpressionClause( MethodDeclarationSyntax declaration, ArrowExpressionClauseSyntax arrow ) =>
			declaration
				.WithBody( null )
				.WithExpressionBody( arrow )
				.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
	}
}