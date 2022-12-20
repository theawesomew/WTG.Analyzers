using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class ParenthesizedExpressionSyntaxExtensions
	{
		public static ExpressionSyntax? GetExpression (this ParenthesizedExpressionSyntax expression)
		{
			var unwrappedExpression = expression.Expression;

			while (unwrappedExpression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				unwrappedExpression = ((ParenthesizedExpressionSyntax)unwrappedExpression).Expression;
			}

			return unwrappedExpression;
		}
	}
}