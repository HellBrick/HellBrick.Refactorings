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
		private static readonly SyntaxToken _objToken = Identifier( "obj" );

		public static ClassDeclarationSyntax OverrideEquals( this ClassDeclarationSyntax classDeclaration, GeneratedPropertyInfo[] properties )
			=> classDeclaration.AddMembers( BuildEqualsOverride( classDeclaration.Identifier, properties ) );

		private static MethodDeclarationSyntax BuildEqualsOverride( SyntaxToken typeName, GeneratedPropertyInfo[] properties )
			=> MethodDeclaration( _boolTypeName, nameof( Object.Equals ) )
			.AddParameterListParameters( Parameter( _objToken ).WithType( ParseTypeName( nameof( Object ) ) ) )
			.AddModifiers( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.OverrideKeyword ) )
			.WithExpressionBody( ArrowExpressionClause( BuildNonGenericEqualsExpression( typeName, properties ) ) )
			.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		private static ExpressionSyntax BuildNonGenericEqualsExpression( SyntaxToken typeName, GeneratedPropertyInfo[] properties )
			=> InvocationExpression
			(
				MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( nameof( IEquatable<object>.Equals ) ) ),
				ArgumentList().AddArguments( Argument( ObjAsType( typeName ) ) )
			);

		private static BinaryExpressionSyntax ObjAsType( SyntaxToken typeName )
			=> BinaryExpression( SyntaxKind.AsExpression, IdentifierName( _objToken ), ParseTypeName( typeName.Text ) );
	}
}
