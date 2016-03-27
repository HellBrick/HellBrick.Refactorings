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
	}
}
