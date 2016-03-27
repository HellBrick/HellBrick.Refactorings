using System;
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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using HellBrick.Refactorings.Utils;

namespace HellBrick.Refactorings.ExpressionBodies
{
	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodiedMethodRefactoring ) ), Shared]
	public class ToExpressionBodiedMethodRefactoring : AbstractExpressionBodyRefactoring<MethodDeclarationSyntax>
	{
		public ToExpressionBodiedMethodRefactoring() : base( new MethodExpressionBodyHandler() )
		{
		}
	}

	[ExportCodeRefactoringProvider( LanguageNames.CSharp, Name = nameof( ToExpressionBodiedMethodRefactoring ) ), Shared]
	public class ToExpressionBodiedOperatorRefactoring : AbstractExpressionBodyRefactoring<OperatorDeclarationSyntax>
	{
		public ToExpressionBodiedOperatorRefactoring() : base( new OperatorExpressionBodyHandler() )
		{
		}
	}
}