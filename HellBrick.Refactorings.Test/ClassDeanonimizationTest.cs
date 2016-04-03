using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.Refactorings.ClassDeanonimization;
using Xunit;

namespace HellBrick.Refactorings.Test
{
	public class ClassDeanonimizationTest
	{
		private readonly ConvertToNestedClassRefactoring _refactoring = new ConvertToNestedClassRefactoring();

		[Fact]
		public void EmptyClass()
		{
			const string sourceCode =
@"using System;
class C
{
	object M() => new {};
}";
			const string expectedCode =
@"using System;
class C
{
	object M() => new __GeneratedClass { };

	private class __GeneratedClass : IEquatable<__GeneratedClass>
	{
		public override string ToString() => ""{}"";
		public override int GetHashCode() => 0;
		public bool Equals( __GeneratedClass other ) => !ReferenceEquals( other, null );
		public override bool Equals( object obj ) => Equals( obj as __GeneratedClass );

		public static bool operator ==( __GeneratedClass x, __GeneratedClass y ) => ReferenceEquals( x, y ) || !ReferenceEquals( x, null ) && x.Equals( y );
		public static bool operator !=( __GeneratedClass x, __GeneratedClass y ) => !( x == y );
	}
}";
			_refactoring.ShouldProvideRefactoring( sourceCode, expectedCode );
		}

		[Fact]
		public void ExplicitlyNamedProperty()
		{
			const string sourceCode =
@"using System;
using System.Collections.Generic;
class C
{
	object M() => new { Value = 42 };
}";
			const string expectedCode =
@"using System;
using System.Collections.Generic;
class C
{
	object M() => new __GeneratedClass { Value = 42 };

	private class __GeneratedClass : IEquatable<__GeneratedClass>
	{
		public int Value { get; set; }

		public override string ToString() => $""{{{nameof( Value )}: {Value}}}"";
		public override int GetHashCode() => EqualityComparer<int>.Default.GetHashCode( Value );
		public bool Equals( __GeneratedClass other ) => !ReferenceEquals( other, null ) && EqualityComparer<int>.Default.Equals( Value, other.Value );
		public override bool Equals( object obj ) => Equals( obj as __GeneratedClass );

		public static bool operator ==( __GeneratedClass x, __GeneratedClass y ) => ReferenceEquals( x, y ) || !ReferenceEquals( x, null ) && x.Equals( y );
		public static bool operator !=( __GeneratedClass x, __GeneratedClass y ) => !( x == y );
	}
}";
			_refactoring.ShouldProvideRefactoring( sourceCode, expectedCode );
		}

		[Fact]
		public void ImplicitlyNamedProperty()
		{
			const string sourceCode =
@"using System;
using System.Collections.Generic;
class C
{
	object M() => new { String.Empty };
}";
			const string expectedCode =
@"using System;
using System.Collections.Generic;
class C
{
	object M() => new __GeneratedClass { Empty = String.Empty };

	private class __GeneratedClass : IEquatable<__GeneratedClass>
	{
		public string Empty { get; set; }

		public override string ToString() => $""{{{nameof( Empty )}: {Empty}}}"";
		public override int GetHashCode() => EqualityComparer<string>.Default.GetHashCode( Empty );
		public bool Equals( __GeneratedClass other ) => !ReferenceEquals( other, null ) && Empty == other.Empty;
		public override bool Equals( object obj ) => Equals( obj as __GeneratedClass );

		public static bool operator ==( __GeneratedClass x, __GeneratedClass y ) => ReferenceEquals( x, y ) || !ReferenceEquals( x, null ) && x.Equals( y );
		public static bool operator !=( __GeneratedClass x, __GeneratedClass y ) => !( x == y );
	}
}";
			_refactoring.ShouldProvideRefactoring( sourceCode, expectedCode );
		}

		[Fact]
		public void MultipleProperties()
		{
			const string sourceCode =
@"using System;
using System.Collections.Generic;
class C
{
	object M() => new { Text = ""42"", String.Empty };
}";
			const string expectedCode =
@"using System;
using System.Collections.Generic;
class C
{
	object M() => new __GeneratedClass { Text = ""42"", Empty = String.Empty };

	private class __GeneratedClass : IEquatable<__GeneratedClass>
	{
		public string Text { get; set; }
		public string Empty { get; set; }

		public override string ToString() => $""{{{nameof( Text )}: {Text}. {nameof( Empty )}: {Empty}}}"";

		public override int GetHashCode()
		{
			unchecked
			{
				const int prime = -1521134295;
				int hash = nameof( __GeneratedClass ).GetHashCode();
				hash = hash * prime + EqualityComparer<string>.Default.GetHashCode( Text );
				hash = hash * prime + EqualityComparer<string>.Default.GetHashCode( Empty );
				return hash;
			}
		}

		public bool Equals( __GeneratedClass other ) => !ReferenceEquals( other, null ) && Text == other.Text && Empty == other.Empty;
		public override bool Equals( object obj ) => Equals( obj as __GeneratedClass );

		public static bool operator ==( __GeneratedClass x, __GeneratedClass y ) => ReferenceEquals( x, y ) || !ReferenceEquals( x, null ) && x.Equals( y );
		public static bool operator !=( __GeneratedClass x, __GeneratedClass y ) => !( x == y );
	}
}";
			_refactoring.ShouldProvideRefactoring( sourceCode, expectedCode );
		}
	}
}
