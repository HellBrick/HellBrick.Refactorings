﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HellBrick.Refactorings.ExpressionBodies
{
	public interface IExpressionBodyHandler<TDeclaration> where TDeclaration : MemberDeclarationSyntax
	{
		bool CanConvertToExpression( TDeclaration declaration );
		BlockSyntax GetBody( TDeclaration declaration );
		string GetIdentifierName( TDeclaration declaration );
		SyntaxNode GetRemovedBlock( TDeclaration declaration );
		TDeclaration ReplaceBodyWithExpressionClause( TDeclaration declaration, ArrowExpressionClauseSyntax arrow );
		ArrowExpressionClauseSyntax GetArrow( TDeclaration member );
		StatementSyntax CreateStatement( ExpressionSyntax expression, TDeclaration declaration );
		TDeclaration ReplaceExpressionClauseWithBody( TDeclaration declaration, BlockSyntax body );
	}
}
