﻿using System;
using System.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using HellBrick.Refactorings.Utils;

namespace HellBrick.Refactorings.VarConversions
{
	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( VarConversionRefactoring ) ), Shared]
	public class VarConversionRefactoring : CodeRefactoringProvider
	{
		private readonly IDeclarationConverter[] converters = new IDeclarationConverter[]
		{
			new VarToExplicitDeclarationConverter(),
			new ExplicitToVarDeclarationConverter()
		};

		public sealed override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
		{
			var root = await context.Document.GetSyntaxRootAsync( context.CancellationToken ).ConfigureAwait( false );
			var semanticModel = await context.Document.GetSemanticModelAsync( context.CancellationToken ).ConfigureAwait( false );
			var declarationTypes =
				EnumerableExtensions.Concat
				(
					root.EnumerateSelectedNodes<LocalDeclarationStatementSyntax>( context.Span ).Select( d => d.Declaration.Type ),
					root.EnumerateSelectedNodes<ForEachStatementSyntax>( context.Span ).Select( f => f.Type ),
					root.EnumerateSelectedNodes<UsingStatementSyntax>( context.Span ).Select( u => u.Declaration?.Type )
				)
				.Where( t => t != null )
				.ToArray();

			foreach ( var converter in converters )
			{
				var supportedDeclarations = declarationTypes.Where( d => converter.CanConvert( d, semanticModel ) ).ToArray();
				if ( supportedDeclarations.Length > 0 )
				{
					CodeAction refactoring = CodeAction.Create( converter.Title, cancelToken => ConvertDocumentAsync( converter, context.Document, root, semanticModel, declarationTypes ) );
					context.RegisterRefactoring( refactoring );
				}
			}
		}

		private Task<Document> ConvertDocumentAsync( IDeclarationConverter converter, Document document, SyntaxNode root, SemanticModel semanticModel, TypeSyntax[] declarations )
		{
			var newRoot = root.ReplaceNodes( declarations, ( original, second ) => ConvertType( converter, semanticModel, original ) );
			var newDocument = document.WithSyntaxRoot( newRoot );
			return Task.FromResult( newDocument );
		}

		private SyntaxNode ConvertType( IDeclarationConverter converter, SemanticModel semanticModel, TypeSyntax original )
		{
			return SyntaxFactory.IdentifierName( converter.ConvertTypeName( original, semanticModel ) )
				.WithLeadingTrivia( original.GetLeadingTrivia() )
				.WithTrailingTrivia( original.GetTrailingTrivia() );
		}
	}
}