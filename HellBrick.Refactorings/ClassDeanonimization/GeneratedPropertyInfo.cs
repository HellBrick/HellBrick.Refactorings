using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace HellBrick.Refactorings.ClassDeanonimization
{
	internal struct GeneratedPropertyInfo : IEquatable<GeneratedPropertyInfo>
	{
		public GeneratedPropertyInfo( INamedTypeSymbol type, string name )
		{
			Type = type;
			Name = name;
		}

		public INamedTypeSymbol Type { get; }
		public string Name { get; }

		public override string ToString() => $"{Type.ToDisplayString()} {Name}";

		public override int GetHashCode()
		{
			unchecked
			{
				const int prime = -1521134295;
				int hash = 12345701;
				hash = hash * prime + EqualityComparer<INamedTypeSymbol>.Default.GetHashCode( Type );
				hash = hash * prime + EqualityComparer<string>.Default.GetHashCode( Name );
				return hash;
			}
		}

		public bool Equals( GeneratedPropertyInfo other ) => EqualityComparer<INamedTypeSymbol>.Default.Equals( Type, other.Type ) && Name == other.Name;
		public override bool Equals( object obj ) => obj is GeneratedPropertyInfo && Equals( (GeneratedPropertyInfo) obj );

		public static bool operator ==( GeneratedPropertyInfo x, GeneratedPropertyInfo y ) => x.Equals( y );
		public static bool operator !=( GeneratedPropertyInfo x, GeneratedPropertyInfo y ) => !x.Equals( y );
	}
}
