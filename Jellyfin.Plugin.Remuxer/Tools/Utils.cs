using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Data.Entities.Libraries;
using NLanguageTag;

namespace Jellyfin.Plugin.Remuxer.Tools
{
    /// <summary>
    /// A common utility lib for remuxing.
    /// </summary>
    public static class Utils
    {
        private static readonly HashSet<char> _invalidFileNameChars = new(Path.GetInvalidFileNameChars());

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

        /// <summary>
        /// Removes invalid characters from strings used in paths.
        /// </summary>
        /// <param name="inputString">String to parse.</param>
        /// <returns>Parsed String.</returns>
        public static string RemoveInvalidFileNameChars(this string? inputString)
        {
            var outputString = string.Empty;
            if (inputString != null)
            {
                outputString = string.Concat(
                    inputString
                    .Select(c => _invalidFileNameChars.Contains(c) ? ' ' : c)
                    .Prepend('.'));
            }

            return outputString;
        }

        /// <summary>
        /// Converts codec to accompanying file extension.
        /// </summary>
        /// <param name="codec">Input codec string.</param>
        /// <returns>File extension.</returns>
        public static string GetFileExtFromCodec(string codec)
        {
            var isSRT = codec.Contains("SRT", StringComparison.OrdinalIgnoreCase); // .srt
            var isSSA = codec.Contains("SubStationAlpha", StringComparison.OrdinalIgnoreCase); // .ssa
            var isASS = codec.Contains("ASS", StringComparison.OrdinalIgnoreCase); // .ass
            var isPgsSubtitle = codec.Contains("PGS", StringComparison.OrdinalIgnoreCase); // .sub
            var isVobSubSubtitle = codec.Contains("VOB", StringComparison.OrdinalIgnoreCase); // .sup

            var fileExtension = "unknown";

            if (isSRT)
            {
                fileExtension = "srt";
            }
            else if (isSSA)
            {
                fileExtension = "ssa";
            }
            else if (isASS)
            {
                fileExtension = "ass";
            }
            else if (isPgsSubtitle)
            {
                fileExtension = "sub";
            }
            else if (isVobSubSubtitle)
            {
                fileExtension = "sup";
            }

            return fileExtension;
        }
    }
}
