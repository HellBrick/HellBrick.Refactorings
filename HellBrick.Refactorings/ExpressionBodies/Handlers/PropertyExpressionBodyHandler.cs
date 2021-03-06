﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace HellBrick.Refactorings.ExpressionBodies
{
	public class PropertyExpressionBodyHandler : IExpressionBodyHandler<PropertyDeclarationSyntax>
	{
		public bool CanConvertToExpression( PropertyDeclarationSyntax declaration )
		{
			AccessorDeclarationSyntax getter = GetAccessor( declaration, SyntaxKind.GetAccessorDeclaration );
			AccessorDeclarationSyntax setter = GetAccessor( declaration, SyntaxKind.SetAccessorDeclaration );

			return getter != null && setter == null;
		}

		public BlockSyntax GetBody( PropertyDeclarationSyntax declaration ) => GetAccessor( declaration, SyntaxKind.GetAccessorDeclaration ).Body;
		public string GetIdentifierName( PropertyDeclarationSyntax declaration ) => declaration.Identifier.Text;
		public SyntaxNode GetRemovedBlock( PropertyDeclarationSyntax declaration ) => declaration.AccessorList;

		public PropertyDeclarationSyntax ReplaceBodyWithExpressionClause( PropertyDeclarationSyntax declaration, ArrowExpressionClauseSyntax arrow ) =>
			declaration
				.WithAccessorList( null )
				.WithExpressionBody( arrow )
				.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) );

		private AccessorDeclarationSyntax GetAccessor( PropertyDeclarationSyntax declaration, SyntaxKind accessorKind )
		{
			return declaration.AccessorList?.Accessors.FirstOrDefault( a => a.IsKind( accessorKind ) );
		}

		public ArrowExpressionClauseSyntax GetArrow( PropertyDeclarationSyntax member ) => member.ExpressionBody;
		public StatementSyntax CreateStatement( ExpressionSyntax expression, PropertyDeclarationSyntax declaration ) => ReturnStatement( expression );

		public PropertyDeclarationSyntax ReplaceExpressionClauseWithBody( PropertyDeclarationSyntax declaration, BlockSyntax body )
			=> declaration
				.WithExpressionBody( null )
				.WithSemicolonToken( Token( SyntaxKind.None ) )
				.WithAccessorList
				(
					AccessorList
					(
						new SyntaxList<AccessorDeclarationSyntax>().Add( AccessorDeclaration( SyntaxKind.GetAccessorDeclaration, body ) )
					)
				);
	}
}
