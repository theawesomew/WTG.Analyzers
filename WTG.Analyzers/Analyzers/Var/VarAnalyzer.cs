﻿using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class VarAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new[]
		{
			Rules.UseVarWherePossibleRule,
		});

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(
				Analyze,
				SyntaxKind.LocalDeclarationStatement,
				SyntaxKind.ForStatement,
				SyntaxKind.ForEachStatement,
				SyntaxKind.UsingStatement);
		}

		void Analyze(SyntaxNodeAnalysisContext context)
		{
			var candidate = Visitor.Instance.Visit(context.Node);

			if (candidate != null)
			{
				var model = context.SemanticModel;
				var typeSymbol = (ITypeSymbol)model.GetSymbolInfo(candidate.Type, context.CancellationToken).Symbol;
				var expressionType = model.GetTypeInfo(candidate.ValueSource, context.CancellationToken).Type;

				if (typeSymbol != null && expressionType != null)
				{
					if (candidate.Unwrap)
					{
						expressionType = EnumerableUtils.GetElementType(expressionType);

						if (typeSymbol == null)
						{
							return;
						}
					}

					if (expressionType == typeSymbol)
					{
						context.ReportDiagnostic(Rules.CreateUseVarWherePossibleDiagnostic(candidate.Type.GetLocation()));
					}
				}
			}
		}

		sealed class Visitor : CSharpSyntaxVisitor<Candidate>
		{
			public static Visitor Instance => instance;
			static readonly Visitor instance = new Visitor();

			public override Candidate VisitForEachStatement(ForEachStatementSyntax node)
			{
				if (!node.Type.IsVar)
				{
					return new Candidate(node.Type, node.Expression, true);
				}

				return null;
			}

			public override Candidate VisitForStatement(ForStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			public override Candidate VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			public override Candidate VisitUsingStatement(UsingStatementSyntax node)
			{
				return ExtractFromVariableDecl(node.Declaration);
			}

			static Candidate ExtractFromVariableDecl(VariableDeclarationSyntax decl)
			{
				if (!decl.Type.IsVar && decl.Variables.Count == 1)
				{
					var exp = decl.Variables[0].Initializer?.Value;

					if (exp != null)
					{
						return new Candidate(decl.Type, exp, false);
					}
				}

				return null;
			}
		}

		sealed class Candidate
		{
			public Candidate(TypeSyntax type, ExpressionSyntax valueSource, bool unwrap)
			{
				if (type == null) throw new ArgumentNullException(nameof(type));
				if (valueSource == null) throw new ArgumentNullException(nameof(valueSource));

				Type = type;
				ValueSource = valueSource;
				Unwrap = unwrap;
			}

			public TypeSyntax Type { get; }
			public ExpressionSyntax ValueSource { get; }
			public bool Unwrap { get; }
		}
	}
}
