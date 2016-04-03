using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellBrick.Refactorings
{
	public static partial class StringExtensions
	{
		public static string NullIfEmpty( this string @string ) => String.IsNullOrEmpty( @string ) ? null : @string;
	}
}
