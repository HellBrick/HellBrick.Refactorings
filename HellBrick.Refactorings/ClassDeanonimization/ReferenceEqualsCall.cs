using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HellBrick.Refactorings.ClassDeanonimization
{
	internal static class ReferenceEqualsCall
	{
		private static readonly LiteralExpressionSyntax _null = LiteralExpression( SyntaxKind.NullLiteralExpression );

		public static ExpressionSyntax IsNotNull( ExpressionSyntax argumentExpression )
			=> PrefixUnaryExpression
			(
				SyntaxKind.LogicalNotExpression,
				Token( SyntaxKind.ExclamationToken ),
				Equal( argumentExpression, _null )
			);

		public static ExpressionSyntax Equal( ExpressionSyntax arg1, ExpressionSyntax arg2 )
			=> InvocationExpression
			(
				MemberAccessExpression
				(
					SyntaxKind.SimpleMemberAccessExpression,
					IdentifierName( nameof( Object ) ),
					IdentifierName( nameof( Object.ReferenceEquals ) )
				)
			)
			.AddArgumentListArguments( Argument( arg1 ), Argument( arg2 ) );
	}
}
