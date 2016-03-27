using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HellBrick.Refactorings.ExpressionBodies
{
	public class MethodExpressionBodyHandler : BaseMethodExpressionBodyHandler<MethodDeclarationSyntax>
	{
		public override string GetIdentifierName( MethodDeclarationSyntax declaration ) => declaration.Identifier.Text;

		public override MethodDeclarationSyntax ReplaceBodyWithExpressionClause( MethodDeclarationSyntax declaration, ArrowExpressionClauseSyntax arrow ) =>
			declaration
				.WithBody( null )
				.WithExpressionBody( arrow )
				.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		public override ArrowExpressionClauseSyntax GetArrow( MethodDeclarationSyntax member ) => member.ExpressionBody;

		public override StatementSyntax CreateStatement( ExpressionSyntax expression, MethodDeclarationSyntax declaration )
			=> declaration.ReturnType.ToString() == "void" ? ExpressionStatement( expression ) as StatementSyntax : ReturnStatement( expression ) as StatementSyntax;

		public override MethodDeclarationSyntax ReplaceExpressionClauseWithBody( MethodDeclarationSyntax declaration, BlockSyntax body )
			=> declaration
				.WithExpressionBody( null )
				.WithSemicolonToken( Token( SyntaxKind.None ) )
				.WithBody( body );
	}
}
