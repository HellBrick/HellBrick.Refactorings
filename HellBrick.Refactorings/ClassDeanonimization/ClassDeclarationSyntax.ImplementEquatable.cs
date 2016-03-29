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
		private static readonly TypeSyntax _boolTypeName = ParseTypeName( "bool" );
		private static readonly SyntaxToken _otherArg = ParseToken( "other" );

		public static ClassDeclarationSyntax ImplementEquatable( this ClassDeclarationSyntax classDeclaration, GeneratedPropertyInfo[] properties )
			=> classDeclaration
			.AddBaseListTypes( SimpleBaseType( ParseTypeName( $"System.IEquatable<{classDeclaration.Identifier}>" ) ) )
			.AddMembers( BuildGenericEqualsMethod( classDeclaration.Identifier, properties ) );

		private static MethodDeclarationSyntax BuildGenericEqualsMethod( SyntaxToken typeName, GeneratedPropertyInfo[] properties )
			=> MethodDeclaration( _boolTypeName, nameof( IEquatable<object>.Equals ) )
			.WithModifiers( TokenList( Token( SyntaxKind.PublicKeyword ) ) )
			.AddParameterListParameters( Parameter( _otherArg ).WithType( ParseTypeName( typeName.Text ) ) )
			.WithExpressionBody( ArrowExpressionClause( BuildEqualsBodyExpression( properties ) ) )
			.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		private static ExpressionSyntax BuildEqualsBodyExpression( GeneratedPropertyInfo[] properties )
			=> properties
			.Select( property => BuildFieldEqualityCall( property ) )
			.Aggregate
			(
				ReferenceEqualsCall.IsNotNull( IdentifierName( _otherArg ) ),
				( old, current ) => BinaryExpression( SyntaxKind.LogicalAndExpression, old, current )
			);

		private static ExpressionSyntax BuildFieldEqualityCall( GeneratedPropertyInfo property )
			=> DeclaresEqualityOperator( property.Type ) ? BuildEqualityOperatorCall( property ) : BuildEqualityComparerEqualsCall( property );

		private static bool DeclaresEqualityOperator( INamedTypeSymbol type )
			=> type
			.GetMembers()
			.OfType<IMethodSymbol>()
			.Where( m => m.MethodKind == MethodKind.BuiltinOperator || m.MethodKind == MethodKind.UserDefinedOperator )
			.Where( m => m.Name == "op_Equality" )
			.Any();

		private static ExpressionSyntax BuildEqualityOperatorCall( GeneratedPropertyInfo property )
			=> BinaryExpression
			(
				SyntaxKind.EqualsExpression,
				MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( property.Name ) ),
				MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, IdentifierName( _otherArg ), IdentifierName( property.Name ) )
			);

		private static ExpressionSyntax BuildEqualityComparerEqualsCall( GeneratedPropertyInfo property )
			=> InvocationExpression
			(
				MemberAccessExpression
				(
					SyntaxKind.SimpleMemberAccessExpression,
					DefaultEqualityComparer.AccessExpression( property.Type ),
					IdentifierName( nameof( EqualityComparer<object>.Default.Equals ) )
				)
			)
			.AddArgumentListArguments
			(
				Argument( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( property.Name ) ) ),
				Argument( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, IdentifierName( _otherArg ), IdentifierName( property.Name ) ) )
			);
	}
}
