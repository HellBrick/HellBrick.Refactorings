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
	public class OperatorExpressionBodyHandler : BaseMethodExpressionBodyHandler<OperatorDeclarationSyntax>
	{
		public override string GetIdentifierName( OperatorDeclarationSyntax declaration ) => "operator " + declaration.OperatorToken.ToString();

		public override OperatorDeclarationSyntax ReplaceBodyWithExpressionClause( OperatorDeclarationSyntax declaration, ArrowExpressionClauseSyntax arrow ) =>
			declaration
				.WithBody( null )
				.WithExpressionBody( arrow )
				.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		public override ArrowExpressionClauseSyntax GetArrow( OperatorDeclarationSyntax member ) => member.ExpressionBody;
		public override StatementSyntax CreateStatement( ExpressionSyntax expression, OperatorDeclarationSyntax declaration ) => ReturnStatement( expression );

		public override OperatorDeclarationSyntax ReplaceExpressionClauseWithBody( OperatorDeclarationSyntax declaration, BlockSyntax body )
			=> declaration
				.WithExpressionBody( null )
				.WithSemicolonToken( Token( SyntaxKind.None ) )
				.WithBody( body );
	}
}
