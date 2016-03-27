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
	}
}
