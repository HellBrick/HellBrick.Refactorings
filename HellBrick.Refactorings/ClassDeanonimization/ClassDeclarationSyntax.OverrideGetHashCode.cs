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
			=> classDeclaration.AddMembers( BuildGetHashCodeOverride( propertySources ) );

		public static MethodDeclarationSyntax BuildGetHashCodeOverride( GeneratedPropertyInfo[] propertySources )
		{
			MethodDeclarationSyntax method =
				MethodDeclaration( ParseTypeName( nameof( Int32 ) ), _getHashCodeIdentifier.Identifier )
				.WithModifiers( SyntaxTokenList.Create( Token( SyntaxKind.PublicKeyword ) ).Add( Token( SyntaxKind.OverrideKeyword ) ) )
				.WithLeadingTrivia( EndOfLine( Environment.NewLine ) );

			method = propertySources.Length < 2 ?
				method.WithExpressionBody( BuildGetHashCodeArrow( propertySources ) ).WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) :
				method.WithBody( BuildGetHashCodeBody( propertySources ) );

			return method;
		}

		private static ArrowExpressionClauseSyntax BuildGetHashCodeArrow( GeneratedPropertyInfo[] propertySources )
			=> ArrowExpressionClause
			(
				propertySources.Length == 0 ?
					LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( 0 ) ) :
					BuildPropertyHashCodeCall( propertySources[ 0 ] )
			);

		private static BlockSyntax BuildGetHashCodeBody( GeneratedPropertyInfo[] propertySources )
			=> Block( CheckedStatement( SyntaxKind.UncheckedStatement, Block( EnumerateHashAlgorithmStatements( propertySources ) ) ) );


		private static IEnumerable<StatementSyntax> EnumerateHashAlgorithmStatements( GeneratedPropertyInfo[] propertySources )
		{
			const string primeName = "prime";
			const string hashName = "hash";
			const int prime = unchecked((int) 2773833001);

			int propNameHash = propertySources
				.Select( p => p.Name )
				.Aggregate( 12345701, ( oldHash, name ) => unchecked(oldHash * prime + name.GetHashCode()) );

			yield return IntDeclaration( primeName, prime ).AddModifiers( Token( SyntaxKind.ConstKeyword ) );
			yield return IntDeclaration( hashName, propNameHash );

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
