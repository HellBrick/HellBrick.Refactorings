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
	internal static partial class ClassDeclarationSyntaxExtensions
	{
		public static ClassDeclarationSyntax OverrideToString( this ClassDeclarationSyntax classDeclaration, GeneratedPropertyInfo[] properties )
			=> classDeclaration.AddMembers( BuildToStringOverride( properties ) );

		private static MethodDeclarationSyntax BuildToStringOverride( GeneratedPropertyInfo[] properties )
			=> MethodDeclaration( ParseTypeName( nameof( String ) ), nameof( Object.ToString ) )
			.AddModifiers( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.OverrideKeyword ) )
			.WithExpressionBody( ArrowExpressionClause( BuildToStringExpression( properties ) ) )
			.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		private static ExpressionSyntax BuildToStringExpression( GeneratedPropertyInfo[] properties )
			=> properties.Length == 0 ? LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( "{}" ) ) : AggregatePropertyToString( properties );

		private static ExpressionSyntax AggregatePropertyToString( GeneratedPropertyInfo[] properties )
			=> InterpolatedStringExpression( Token( SyntaxKind.InterpolatedStringStartToken ) )
			.AddContents( InterpolatedText( "{{" ) )
			.AddContents( EnumeratePropertyInterpolationFragments( properties ).ToArray() )
			.AddContents( InterpolatedText( "}}" ) );

		private static IEnumerable<InterpolatedStringContentSyntax> EnumeratePropertyInterpolationFragments( GeneratedPropertyInfo[] properties )
		{
			bool isFirstAppended = false;

			foreach ( GeneratedPropertyInfo property in properties )
			{
				if ( isFirstAppended )
					yield return InterpolatedText( ". " );

				foreach ( InterpolatedStringContentSyntax propertyContent in EnumeratePropertyInterpolationContent( property ) )
					yield return propertyContent;

				isFirstAppended = true;
			}
		}

		private static IEnumerable<InterpolatedStringContentSyntax> EnumeratePropertyInterpolationContent( GeneratedPropertyInfo property )
		{
			yield return Interpolation( InvocationExpression( IdentifierName( "nameof" ) ).AddArgumentListArguments( Argument( IdentifierName( property.Name ) ) ) );
			yield return InterpolatedText( ": " );
			yield return Interpolation( IdentifierName( property.Name ) );
		}

		private static InterpolatedStringTextSyntax InterpolatedText( string text )
			=> InterpolatedStringText( Token( TriviaList(), SyntaxKind.InterpolatedStringTextToken, text, text, TriviaList() ) );
	}
}
