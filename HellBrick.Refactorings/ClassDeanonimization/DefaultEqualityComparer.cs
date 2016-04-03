using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HellBrick.Refactorings.ClassDeanonimization
{
	internal static class DefaultEqualityComparer
	{
		public static MemberAccessExpressionSyntax AccessExpression( ITypeSymbol propertyType )
			=> MemberAccessExpression
			(
				SyntaxKind.SimpleMemberAccessExpression,
				ParseTypeName( $"System.Collections.Generic.EqualityComparer<{propertyType.ToDisplayString()}>" ),
				IdentifierName( nameof( EqualityComparer<object>.Default ) )
			);
	}
}
