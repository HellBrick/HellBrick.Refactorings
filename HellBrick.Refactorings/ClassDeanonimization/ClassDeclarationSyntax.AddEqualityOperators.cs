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
		private static readonly SyntaxToken _xToken = Identifier( "x" );
		private static readonly SyntaxToken _yToken = Identifier( "y" );

		public static ClassDeclarationSyntax AddEqualityOperators( this ClassDeclarationSyntax classDeclaration, GeneratedPropertyInfo[] properties )
			=> classDeclaration.AddMembers
			(
				BuildEqualsOperator( classDeclaration.Identifier, properties ),
				BuildNotEqualsOperator( classDeclaration.Identifier, properties )
			);

		private static OperatorDeclarationSyntax BuildEqualsOperator( SyntaxToken typeName, GeneratedPropertyInfo[] propertySources )
			=> OperatorDeclaration( _boolTypeName, Token( SyntaxKind.EqualsEqualsToken ) )
			.AddModifiers( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.StaticKeyword ) )
			.AddParameterListParameters
			(
				Parameter( _xToken ).WithType( ParseTypeName( typeName.Text ) ),
				Parameter( _yToken ).WithType( ParseTypeName( typeName.Text ) )
			)
			.WithExpressionBody
			(
				ArrowExpressionClause
				(
					BinaryExpression
					(
						SyntaxKind.LogicalOrExpression,
						ReferenceEqualsCall.Equal( IdentifierName( _xToken ), IdentifierName( _yToken ) ),
						BinaryExpression
						(
							SyntaxKind.LogicalAndExpression,
							ReferenceEqualsCall.IsNotNull( IdentifierName( _xToken ) ),
							InvocationExpression
							(
								MemberAccessExpression
								(
									SyntaxKind.SimpleMemberAccessExpression, IdentifierName( _xToken ), IdentifierName( nameof( IEquatable<object>.Equals ) )
								),
								ArgumentList().AddArguments( Argument( IdentifierName( _yToken ) ) )
							)
						)
					)
				)
			)
			.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		private static OperatorDeclarationSyntax BuildNotEqualsOperator( SyntaxToken typeName, GeneratedPropertyInfo[] propertySources )
			=> OperatorDeclaration( _boolTypeName, Token( SyntaxKind.ExclamationEqualsToken ) )
			.AddModifiers( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.StaticKeyword ) )
			.AddParameterListParameters
			(
				Parameter( _xToken ).WithType( ParseTypeName( typeName.Text ) ),
				Parameter( _yToken ).WithType( ParseTypeName( typeName.Text ) )
			)
			.WithExpressionBody
			(
				ArrowExpressionClause
				(
					PrefixUnaryExpression
					(
						SyntaxKind.LogicalNotExpression,
						ParenthesizedExpression
						(
							BinaryExpression
							(
								SyntaxKind.EqualsExpression,
								IdentifierName( _xToken ),
								IdentifierName( _yToken )
							)
						)
					)
				)
			)
			.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );
	}
}
