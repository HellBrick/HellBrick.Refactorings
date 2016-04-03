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
		private static readonly IdentifierNameSyntax _getHashCodeIdentifier = IdentifierName( nameof( Object.GetHashCode ) );

		public static ClassDeclarationSyntax OverrideGetHashCode( this ClassDeclarationSyntax classDeclaration, GeneratedPropertyInfo[] propertySources )
			=> classDeclaration.AddMembers( BuildGetHashCodeOverride( classDeclaration.Identifier, propertySources ) );

		public static MethodDeclarationSyntax BuildGetHashCodeOverride( SyntaxToken className, GeneratedPropertyInfo[] propertySources )
		{
			MethodDeclarationSyntax method =
				MethodDeclaration( ParseTypeName( nameof( Int32 ) ), _getHashCodeIdentifier.Identifier )
				.WithModifiers( SyntaxTokenList.Create( Token( SyntaxKind.PublicKeyword ) ).Add( Token( SyntaxKind.OverrideKeyword ) ) )
				.WithLeadingTrivia( EndOfLine( Environment.NewLine ) );

			method = propertySources.Length < 2 ?
				method.WithExpressionBody( BuildGetHashCodeArrow( propertySources ) ).WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) :
				method.WithBody( BuildGetHashCodeBody( className, propertySources ) );

			return method;
		}

		private static ArrowExpressionClauseSyntax BuildGetHashCodeArrow( GeneratedPropertyInfo[] propertySources )
			=> ArrowExpressionClause
			(
				propertySources.Length == 0 ?
					LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( 0 ) ) :
					BuildPropertyHashCodeCall( propertySources[ 0 ] )
			);

		private static BlockSyntax BuildGetHashCodeBody( SyntaxToken className, GeneratedPropertyInfo[] propertySources )
			=> Block( CheckedStatement( SyntaxKind.UncheckedStatement, Block( EnumerateHashAlgorithmStatements( className, propertySources ) ) ) );


		private static IEnumerable<StatementSyntax> EnumerateHashAlgorithmStatements( SyntaxToken className, GeneratedPropertyInfo[] propertySources )
		{
			const string primeName = "prime";
			const string hashName = "hash";
			const int prime = unchecked((int) 2773833001);

			yield return IntDeclaration( primeName, prime ).AddModifiers( Token( SyntaxKind.ConstKeyword ) );
			yield return IntDeclaration
			(
				hashName,
				InvocationExpression
				(
					MemberAccessExpression
					(
						SyntaxKind.SimpleMemberAccessExpression,
						InvocationExpression( IdentifierName( "nameof" ) ).AddArgumentListArguments( Argument( IdentifierName( className ) ) ),
						IdentifierName( nameof( String.GetHashCode ) )
					)
				)
			);

			foreach ( GeneratedPropertyInfo property in propertySources )
			{
				AssignmentExpressionSyntax assignment =
					AssignmentExpression
					(
						SyntaxKind.SimpleAssignmentExpression,
						IdentifierName( hashName ),
						BinaryExpression
						(
							SyntaxKind.AddExpression,
							BinaryExpression( SyntaxKind.MultiplyExpression, IdentifierName( hashName ), IdentifierName( primeName ) ),
							BuildPropertyHashCodeCall( property )
						)
					);

				ExpressionStatementSyntax statement = ExpressionStatement( assignment, Token( SyntaxKind.SemicolonToken ) );
				yield return statement;
			}

			yield return ReturnStatement( IdentifierName( hashName ) );
		}

		private static LocalDeclarationStatementSyntax IntDeclaration( string varName, int value )
			=> IntDeclaration( varName, LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( value ) ) );

		private static LocalDeclarationStatementSyntax IntDeclaration( string varName, ExpressionSyntax valueExpression )
			=> LocalDeclarationStatement
			(
				VariableDeclaration( ParseTypeName( "int" ) )
				.AddVariables
				(
					VariableDeclarator( varName ).WithInitializer( EqualsValueClause( valueExpression ) )
				)
			);

		private static ExpressionSyntax BuildPropertyHashCodeCall( GeneratedPropertyInfo propertyInfo )
		{
			MemberAccessExpressionSyntax defaultComparer = DefaultEqualityComparer.AccessExpression( propertyInfo.Type );
			MemberAccessExpressionSyntax defaultGetHashCodeMethod = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, defaultComparer, _getHashCodeIdentifier );
			MemberAccessExpressionSyntax fieldAccess = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( propertyInfo.Name ) );
			InvocationExpressionSyntax getHashCodeCall = InvocationExpression( defaultGetHashCodeMethod ).AddArgumentListArguments( Argument( fieldAccess ) );
			return getHashCodeCall;
		}
	}
}
