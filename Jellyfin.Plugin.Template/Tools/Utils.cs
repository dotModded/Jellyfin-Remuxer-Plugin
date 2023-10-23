using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLanguageTag;

namespace Jellyfin.Plugin.Remuxer.Tools
{
    /// <summary>
    /// A common utility lib for remuxing.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Checks if the language is on the whitelist.
        /// </summary>
        /// <param name="languageTag">Language to compare against the whitelist.</param>
        /// <param name="allowedLanguages">Languages to allow.</param>
        /// <returns>true if the language is in the whitelist, false otherwise.</returns>
        public static bool IsLanguageAllowed(string languageTag, HashSet<string> allowedLanguages)
        {
            // Parse the provided language tag using the external library
            // var parsed = LanguageTag.TryParse(languageTag, out var language);
            // TODO:
            // Check the standardized language code or the original tag against the allowed list
            return allowedLanguages.Contains(languageTag);
        }
    }
}
