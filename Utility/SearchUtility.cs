// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf;

using System;
using System.Runtime.CompilerServices;
using System.Text;

public static class SearchUtility
{
	public static bool Matches(string? input, string[]? query)
	{
		if (input is null)
			return false;

		if (query is null)
			return true;

		// Sanitize the input once:
		string sanitizedInput = Sanitize(input);

		// Check each term
		foreach (string term in query)
		{
			// Ignore "the"
			if (term.Equals("the", StringComparison.OrdinalIgnoreCase))
				continue;

			// Sanitize the term
			string sanitizedTerm = Sanitize(term);

			// If integers, match against numeric form; otherwise match the string
			if (int.TryParse(sanitizedTerm, out int integerValue))
			{
				if (!sanitizedInput.Contains(integerValue.ToString()))
					return false;
			}
			else
			{
				if (!sanitizedInput.Contains(sanitizedTerm))
					return false;
			}
		}

		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string Sanitize(string input)
	{
		// Remove any non-(letter/digit/underscore/whitespace) character, and convert to lowercase
		var sb = new StringBuilder(input.Length);
		foreach (char c in input)
		{
			if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || char.IsWhiteSpace(c))
				sb.Append(char.ToLowerInvariant(c));
		}

		return sb.ToString();
	}
}
