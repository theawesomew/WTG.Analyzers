using System;
using Microsoft.CodeAnalysis;

namespace WTG.Analyzers.Utils
{
	public static class SymbolExtensions
	{
		public static bool IsValueTuple(this ITypeSymbol typeSymbol)
		{
			return typeSymbol.MetadataName.StartsWith("ValueTuple`", StringComparison.Ordinal)
				&& typeSymbol.ContainingType == null
				&& typeSymbol.ContainingNamespace.IsMatch("System");
		}

		public static bool IsMatch(this IMethodSymbol methodSymbol, string fullTypeName, string methodName)
		{
			return methodSymbol.MetadataName == methodName
				&& methodSymbol.ContainingType.IsMatch(fullTypeName);
		}

		public static bool IsMatch(this IMethodSymbol methodSymbol, string assemblyName, string fullTypeName, string methodName)
		{
			return methodSymbol.MetadataName == methodName
				&& methodSymbol.ContainingAssembly.IsMatch(assemblyName)
				&& methodSymbol.ContainingType.IsMatch(fullTypeName);
		}

		public static bool IsMatch(this ITypeSymbol typeSymbol, string fullName)
		{
			ISymbol symbol = typeSymbol;
			var length = fullName.Length;

			while (true)
			{
				var index = fullName.LastIndexOf('+', length - 1);

				if (index < 0)
				{
					break;
				}

				if (string.Compare(symbol.MetadataName, 0, fullName, index + 1, length - index - 1, StringComparison.Ordinal) != 0)
				{
					return false;
				}

				length = index;
				symbol = symbol.ContainingType;

				if (symbol == null)
				{
					return false;
				}
			}

			if (symbol.ContainingType != null)
			{
				return false;
			}

			return IsMatchCore(symbol, fullName, length);
		}

		public static bool IsMatch(this ITypeSymbol typeSymbol, string assemblyName, string fullName)
		{
			return typeSymbol.IsMatch(fullName)
				&& typeSymbol.ContainingAssembly.IsMatch(assemblyName);
		}

		public static bool IsMatch(this INamespaceSymbol namespaceSymbol, string fullName)
		{
			return IsMatchCore(namespaceSymbol, fullName, fullName.Length);
		}

		public static bool IsMatch(this IAssemblySymbol assemblySymbol, string assemblyName)
		{
			return assemblySymbol.Identity.Name == assemblyName;
		}

		static bool IsMatchCore(ISymbol symbol, string fullName, int length)
		{
			while (true)
			{
				var index = fullName.LastIndexOf('.', length - 1);

				if (index < 0)
				{
					break;
				}

				if (string.Compare(symbol.MetadataName, 0, fullName, index + 1, length - index - 1, StringComparison.Ordinal) != 0)
				{
					return false;
				}

				length = index;
				var ns = symbol.ContainingNamespace;

				if (ns.IsGlobalNamespace)
				{
					return false;
				}

				symbol = ns;
			}

			if (string.Compare(symbol.MetadataName, 0, fullName, 0, length, StringComparison.Ordinal) != 0)
			{
				return false;
			}

			var tmp = symbol.ContainingNamespace;

			return tmp.IsGlobalNamespace;
		}
	}
}
