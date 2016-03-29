using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using HellBrick.Refactorings.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Simplification;

namespace HellBrick.Refactorings.ClassDeanonimization
{
	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ConvertToNestedClassRefactoring ) ), Shared]
	public class ConvertToNestedClassRefactoring : CodeRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
		{
			SyntaxNode root = await context.Document.GetSyntaxRootAsync().ConfigureAwait( false );
			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait( false );

			var convertableAnonymousObjects = root
				.EnumerateSelectedNodes<AnonymousObjectCreationExpressionSyntax>( context.Span )
				.Where( anonymousObject => CanConvert( anonymousObject, semanticModel ) );

			foreach ( AnonymousObjectCreationExpressionSyntax anonymousObject in convertableAnonymousObjects )
			{
				CodeAction refactoring = CodeAction.Create( "Convert anonymous class to nested class", ct => ConvertAsync( context.Document, anonymousObject, ct ) );
				context.RegisterRefactoring( refactoring );
			}
		}

		private bool CanConvert( AnonymousObjectCreationExpressionSyntax anonymousObject, SemanticModel semanticModel )
			=> anonymousObject
			.Initializers
			.Select( initializer => GetPropertyType( initializer, semanticModel ) )
			.All( type => type?.IsAnonymousType == false && type.TypeKind != TypeKind.Error );

		private async Task<Document> ConvertAsync( Document document, AnonymousObjectCreationExpressionSyntax anonymousObject, CancellationToken ct )
		{
			SyntaxNode root = await document.GetSyntaxRootAsync( ct ).ConfigureAwait( false );
			SemanticModel semanticModel = await document.GetSemanticModelAsync( ct ).ConfigureAwait( false );

			GeneratedPropertyInfo[] propertySources =
				anonymousObject
				.Initializers
				.Select( ( initializer, index ) => CreatePropertyInfo( initializer, index, semanticModel ) )
				.ToArray();

			ClassDeclarationSyntax nestedClassDeclaration = BuildClass( propertySources );
			ObjectCreationExpressionSyntax objectCreationExpression = BuildObjectCreationExpression( anonymousObject, propertySources, nestedClassDeclaration.Identifier.Text );

			TypeDeclarationSyntax declaringType = anonymousObject.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
			SyntaxNode newRoot = root
				.ReplaceNodes
				(
					new SyntaxNode[] { anonymousObject, declaringType },
					( original, rewritten ) => rewritten == anonymousObject ? objectCreationExpression : InsertNestedClass( rewritten, nestedClassDeclaration )
				);

			return document.WithSyntaxRoot( newRoot );
		}

		private SyntaxNode InsertNestedClass( SyntaxNode rewrittenDeclaringType, ClassDeclarationSyntax nestedClassDeclaration )
		{
			ClassDeclarationSyntax declaringClass = rewrittenDeclaringType as ClassDeclarationSyntax;
			if ( declaringClass != null )
				return declaringClass.AddMembers( nestedClassDeclaration );

			StructDeclarationSyntax declaringStruct = rewrittenDeclaringType as StructDeclarationSyntax;
			return declaringStruct.AddMembers( nestedClassDeclaration );
		}

		private static INamedTypeSymbol GetPropertyType( AnonymousObjectMemberDeclaratorSyntax initializer, SemanticModel semanticModel )
			=> semanticModel.GetTypeInfo( initializer.Expression ).ConvertedType as INamedTypeSymbol;

		private GeneratedPropertyInfo CreatePropertyInfo( AnonymousObjectMemberDeclaratorSyntax initializer, int index, SemanticModel semanticModel )
		{
			INamedTypeSymbol type = GetPropertyType( initializer, semanticModel );

			SyntaxToken nameToken = initializer.NameEquals != null ? initializer.NameEquals.Name.Identifier : ExtractAnonymousTypeMemberName( initializer.Expression );
			string name = nameToken.Text.NullIfEmpty() ?? $"{type.Name}Property{index}";

			return new GeneratedPropertyInfo( type, name );
		}

		private static SyntaxToken ExtractAnonymousTypeMemberName( ExpressionSyntax input )
		{
			while ( true )
			{
				switch ( input.Kind() )
				{
					case SyntaxKind.IdentifierName:
						return ( (IdentifierNameSyntax) input ).Identifier;

					case SyntaxKind.SimpleMemberAccessExpression:
						input = ( (MemberAccessExpressionSyntax) input ).Name;
						continue;

					case SyntaxKind.ConditionalAccessExpression:
						input = ( (ConditionalAccessExpressionSyntax) input ).WhenNotNull;
						if ( input.Kind() == SyntaxKind.MemberBindingExpression )
						{
							return ( (MemberBindingExpressionSyntax) input ).Name.Identifier;
						}

						continue;

					default:
						return default( SyntaxToken );
				}
			}
		}

		private ObjectCreationExpressionSyntax BuildObjectCreationExpression
		(
			AnonymousObjectCreationExpressionSyntax anonymousObject,
			GeneratedPropertyInfo[] propertySources,
			string className
		)
			=> ObjectCreationExpression( ParseTypeName( className ) )
			.WithInitializer
			(
				InitializerExpression( SyntaxKind.ObjectInitializerExpression )
					.WithExpressions
					(
						new SeparatedSyntaxList<ExpressionSyntax>()
							.AddRange
							(
								anonymousObject
									.Initializers
									.Zip
									(
										propertySources,
										( anonymousInitializer, propertySource ) => AssignmentExpression
										(
											SyntaxKind.SimpleAssignmentExpression,
											IdentifierName( propertySource.Name ),
											anonymousInitializer.Expression
										)
									)
							)
					)
			);

		private ClassDeclarationSyntax BuildClass( GeneratedPropertyInfo[] propertySources )
			=> BuildClass( ParseToken( "__GeneratedClass" ), propertySources );

		private ClassDeclarationSyntax BuildClass( SyntaxToken typeName, GeneratedPropertyInfo[] propertySources )
			=> ClassDeclaration( String.Empty )
			.WithIdentifier( typeName.WithAdditionalAnnotations( RenameAnnotation.Create() ) )
			.WithModifiers( SyntaxTokenList.Create( Token( SyntaxKind.PrivateKeyword ) ) )
			.AddMembers( propertySources.Select( p => BuildProperty( p ) ).ToArray() )
			.OverrideToString( propertySources )
			.OverrideGetHashCode( propertySources )
			.ImplementEquatable( propertySources )
			.OverrideEquals( propertySources )
			.AddEqualityOperators( propertySources )
			.WithAdditionalAnnotations( Simplifier.Annotation );

		private PropertyDeclarationSyntax BuildProperty( GeneratedPropertyInfo propertyInfo )
			=> PropertyDeclaration( ParseTypeName( propertyInfo.Type.ToDisplayString() ), propertyInfo.Name )
			.WithModifiers( SyntaxTokenList.Create( Token( SyntaxKind.PublicKeyword ) ) )
			.WithAccessorList
			(
				AccessorList
				(
					new SyntaxList<AccessorDeclarationSyntax>()
						.Add( AccessorDeclaration( SyntaxKind.GetAccessorDeclaration ).WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) )
						.Add( AccessorDeclaration( SyntaxKind.SetAccessorDeclaration ).WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ) )
				)
			);
	}
}