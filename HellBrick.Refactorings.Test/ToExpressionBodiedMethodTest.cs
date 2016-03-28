using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.Refactorings.ExpressionBodies;
using Xunit;

namespace HellBrick.Refactorings.Test
{
	public class ToExpressionBodiedMethodTest
	{
		private readonly ToExpressionBodyRefactoring _provider = new ToExpressionBodyRefactoring();

		[Fact]
		public void MultipleLineMethodIsIgnored()
		{
			const string sourceCode =
@"int Calc()
{
	int x = 2;
	return x * x;
}";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void SingleLineThrowingMethodIsIgnored()
		{
			const string sourceCode = "int Calc() { throw new Exception(); }";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void ExpressionBodiedMethodIsIgnored()
		{
			const string sourceCode = "int Calc() => 42;";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void SingleLineReturningMethodIsConverted()
		{
			const string sourceCode = "int Calc() { return 42; }\r\n";
			const string expected = "int Calc() => 42;\r\n";
			_provider.ShouldProvideRefactoring( sourceCode, expected );
		}

		[Fact]
		public void SingleLineVoidMethodIsConverted()
		{
			const string sourceCode = "void DoStuff() { Console.WriteLine( 42 ); }\r\n";
			const string expected = "void DoStuff() => Console.WriteLine( 42 );\r\n";
			_provider.ShouldProvideRefactoring( sourceCode, expected );
		}
	}
}
