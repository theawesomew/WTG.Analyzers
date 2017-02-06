﻿using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	internal class EnumerableTypeUtilsTest
	{
		[TestCase("int[]", ExpectedResult = "int")]
		[TestCase("IEnumerable<double>", ExpectedResult = "double")]
		[TestCase("IEnumerable", ExpectedResult = "object")]
		[TestCase("PreGenericTypedCollection", ExpectedResult = "float")]
		public string GetItemType(string enumerableType)
		{
			return EnumerableTypeUtils.GetElementType(GetType(enumerableType)).ToString();
		}

		#region Implementation

		[OneTimeSetUp]
		protected async Task Setup()
		{
			string source =
@"using System;
using System.Collections;
using System.Collections.Generic;

class PreGenericTypedCollection : IEnumerable
{
	public Enumerator GetEnumerator() => new Enumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public struct Enumerator : IEnumerator
	{
		public float Current => default(float);
		object IEnumerator.Current => Current;
		public bool MoveNext() => false;

		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}
";

			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
			semanticModel = compilation.GetSemanticModel(tree);
			pos = tree.Length;
		}

		ITypeSymbol GetType(string source)
		{
			var syntax = SyntaxFactory.ParseTypeName(source);
			if (syntax == null) throw new ArgumentException("Parse fail: " + source);

			var info = semanticModel.GetSpeculativeSymbolInfo(pos, syntax, SpeculativeBindingOption.BindAsTypeOrNamespace);
			if (info.Symbol == null) throw new ArgumentException("Semantic fail: " + info.CandidateReason + ": " + source);

			return (ITypeSymbol)info.Symbol;
		}

		SemanticModel semanticModel;
		int pos;

		#endregion
	}
}
