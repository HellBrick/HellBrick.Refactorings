using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.Refactorings.ExpressionBodies;
using Xunit;

namespace HellBrick.Refactorings.Test
{
	public class ToExpressionBodiedPropertyTest
	{
		private readonly ToExpressionBodyRefactoring _provider = new ToExpressionBodyRefactoring();

		[Fact]
		public void MultipleLinePropertyMethodIsIgnored()
		{
			const string sourceCode = "int Value {	get { int x = 2; return x * x; } }";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void PropertyWithSetterIsIgnored()
		{
			const string sourceCode = "int Value {	get { return 42; } set {} }";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void SingleLineThrowingPropertyIsIgnored()
		{
			const string sourceCode = "int Value { get { throw new Exception(); } }";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void ExpressionBodiedPropertyIsIgnored()
		{
			const string sourceCode = "int Calc => 42;";
			_provider.ShouldNotProvideRefactoring( sourceCode );
		}

		[Fact]
		public void SingleLineReturningPropertyIsConverted()
		{
			const string sourceCode = "int Value { get { return 42; } }\r\n";
			const string expected = "int Value => 42;\r\n";
			_provider.ShouldProvideRefactoring( sourceCode, expected );
		}
	}
}
