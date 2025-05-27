// © XIV-Tools.
// Licensed under the MIT license.

namespace XivToolsWpf.DependencyInjection;

/// <summary>
/// Interface for a localization provider, used to manage translations and locale changes.
/// </summary>
public interface ILocaleProvider : IDependency
{
	/// <summary>
	/// Raised when the application's locale (language) changes.
	/// </summary>
	event LocalizationEvent? LocaleChanged;

	/// <summary>
	/// Gets a value indicating whether the locale provider is loaded.
	/// </summary>
	bool Loaded { get; }

	/// <summary>
	/// Checks whether the locale provider has an entry with the specified key.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <returns>True if the key exists, false otherwise.</returns>
	bool HasString(string key);

	/// <summary>
	/// Retrieves a formated translation from the locale provider corresponding to the specified key.
	/// </summary>
	/// <param name="key">The key of the translation.</param>
	/// <param name="param">An array of parameters to format the string with.</param>
	/// <returns>The formatted string.</returns>
	string GetStringFormatted(string key, params string[] param);

	/// <summary>
	/// Retrieves the translations for the specified key in all available languages.
	/// </summary>
	/// <param name="key">The key of the translation(s).</param>
	/// <returns>Concatenated string of translations in all languages.</returns>
	string GetStringAllLanguages(string key);

	/// <summary>
	/// Attempts to retrieve a translation for the specified key from the locale provider.
	/// </summary>
	/// <param name="key">The key of the translation.</param>
	/// <param name="silent">A flag indicating whether to suppress errors if the key is not found.</param>
	/// <returns>The translation string if found; otherwise, an empty string.</returns>
	string GetString(string key, bool silent = false);
}

public delegate void LocalizationEvent();
