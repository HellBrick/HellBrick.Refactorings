﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;

namespace HellBrick.Refactorings.Test
{
	internal static class CodeRefactoringProviderExtensions
	{
		public static void ShouldNotProvideRefactoring( this CodeRefactoringProvider provider, string sourceCode )
		{
			var document = GetDocument( sourceCode );
			var span = new TextSpan( 0, sourceCode.Length );
			var refactorings = GetCodeRefactorings( provider, document, span );
			refactorings.Length.Should().Be( 0 );
		}

		public static void ShouldProvideRefactoring( this CodeRefactoringProvider provider, string sourceCode, string expectedCode )
		{
			var document = GetDocument( sourceCode );
			var span = new TextSpan( 0, sourceCode.Length );
			var refactorings = GetCodeRefactorings( provider, document, span );
			refactorings.Length.Should().Be( 1 );

			var refactoring = refactorings[ 0 ];
			var operations = refactoring.GetOperationsAsync( CancellationToken.None ).GetAwaiter().GetResult();
			operations.Length.Should().Be( 1 );

			var operation = operations[ 0 ];
			var workspace = document.Project.Solution.Workspace;
			operation.Apply( workspace, CancellationToken.None );

			var newDocument = workspace.CurrentSolution.GetDocument( document.Id );
			var newSourceCode = newDocument.GetTextAsync().GetAwaiter().GetResult().ToString();

			newSourceCode.Should().Be( expectedCode );
		}

		private static Document GetDocument( string code )
		{
			var references = ImmutableList.Create<MetadataReference>(
				 MetadataReference.CreateFromFile( typeof( object ).GetType().Assembly.Location ),
				 MetadataReference.CreateFromFile( typeof( Enumerable ).GetType().Assembly.Location ) );

			var adhocWorkspace = new AdhocWorkspace();
			adhocWorkspace.Options = adhocWorkspace.Options.WithProperFormatting();

			return adhocWorkspace
				 .AddProject( "TestProject", LanguageNames.CSharp )
				 .AddMetadataReferences( references )
				 .AddDocument( "TestDocument", code );
		}

		private static ImmutableArray<CodeAction> GetCodeRefactorings( CodeRefactoringProvider provider, Document document, TextSpan span )
		{
			var builder = ImmutableArray.CreateBuilder<CodeAction>();
			Action<CodeAction> registerRefactoring = a => builder.Add( a );

			var context = new CodeRefactoringContext( document, span, registerRefactoring, CancellationToken.None );
			provider.ComputeRefactoringsAsync( context ).GetAwaiter().GetResult();

			return builder.ToImmutable();
		}
	}
}
