using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HellBrick.Refactorings.StringInterpolation
{
	internal struct StringFormatConversion : IEquatable<StringFormatConversion>
	{
		public static readonly StringFormatConversion None = default( StringFormatConversion );

		public InvocationExpressionSyntax FormatCall { get; }
		public InterpolatedStringExpressionSyntax InterpolatedString { get; }

		public bool IsSuccess => FormatCall != null && InterpolatedString != null;

		private StringFormatConversion( InvocationExpressionSyntax formatCall, InterpolatedStringExpressionSyntax interpolatedString )
		{
			FormatCall = formatCall;
			InterpolatedString = interpolatedString;
		}

		public static StringFormatConversion TryCreateConversion( InvocationExpressionSyntax formatCall )
		{
			var formatString = ( formatCall.ArgumentList.Arguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax )?.Token.ValueText;
			if ( formatString == null )
				return None;

			var arguments = formatCall.ArgumentList.Arguments.Skip( 1 ).Select( arg => arg.Expression ).ToList();
			var parts = FormatStringParser.Parse( formatString, arguments );
			if ( parts == null )
				return None;

			InterpolatedStringExpressionSyntax interpolatedString =
				SyntaxFactory.InterpolatedStringExpression
				(
					SyntaxFactory.Token( SyntaxKind.InterpolatedStringStartToken ),
					SyntaxFactory.List<InterpolatedStringContentSyntax>( parts )
				);

			return new StringFormatConversion( formatCall, interpolatedString );
      }

		public override int GetHashCode()
		{
			unchecked
			{
				const int prime = -1521134295;
				int hash = 12345701;
				hash = hash * prime + EqualityComparer<InvocationExpressionSyntax>.Default.GetHashCode( FormatCall );
				hash = hash * prime + EqualityComparer<InterpolatedStringExpressionSyntax>.Default.GetHashCode( InterpolatedString );
				return hash;
			}
		}

		public bool Equals( StringFormatConversion other ) => EqualityComparer<InvocationExpressionSyntax>.Default.Equals( FormatCall, other.FormatCall ) && EqualityComparer<InterpolatedStringExpressionSyntax>.Default.Equals( InterpolatedString, other.InterpolatedString );
		public override bool Equals( object obj ) => obj is StringFormatConversion && Equals( (StringFormatConversion) obj );

		public static bool operator ==( StringFormatConversion x, StringFormatConversion y ) => x.Equals( y );
		public static bool operator !=( StringFormatConversion x, StringFormatConversion y ) => !x.Equals( y );
	}
}
