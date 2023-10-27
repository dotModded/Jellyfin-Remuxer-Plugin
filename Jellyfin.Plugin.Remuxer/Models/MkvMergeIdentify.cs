using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Remuxer.Models
{
    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public static class MkvMergeIdentify
    {
        /// <summary>
        /// Helper functions for interacting with MkvMerge.
        /// </summary>
        /// <param name="path">path to mkv file.</param>
        /// <returns>MkvMergeOutput is a object based representation of the JSON output from mkvmerge.</returns>
        public static MkvMergeOutput? GetMkvInfo(string path)
        {
            var startInfo = new ProcessStartInfo("mkvmerge", $@"-i -F json ""{path}""")
            {
                RedirectStandardOutput = true, // Redirects stdout so we can read it directly
                UseShellExecute = false, // Necessary to redirect IO
                CreateNoWindow = true, // Prevents a command window from popping up
            };

            using var process = Process.Start(startInfo);
            using StreamReader reader = process!.StandardOutput;
            string json = reader.ReadToEnd(); // Reads the JSON output directly into a string

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            return JsonSerializer.Deserialize<MkvMergeOutput>(json, options);
        }
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeAttachment
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? MimeType { get; set; }
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeChapter
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? EditionEntry { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for JSON deserialization")]
        [SuppressMessage("Microsoft.Usage", "CA1002:DoNotExposeGenericLists", Justification = "Required for JSON deserialization")]
        public List<MkvMergeChapterDetail>? Chapters { get; set; }
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeChapterDetail
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? ChapterAtom { get; set; }
        // Other properties based on your JSON structure
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeTrack
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? Codec { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public MkvMergeTrackProperties? Properties { get; set; }
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeTrackProperties
    {
        // Shared

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [JsonPropertyName("language")]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [JsonPropertyName("default_track")]
        public bool? DefaultTrack { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [JsonPropertyName("forced_track")]
        public bool? ForcedTrack { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [JsonPropertyName("track_name")]
        public string? TrackName { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [JsonPropertyName("flag_original")]
        public bool? OriginalTrack { get; set; }
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeContainer
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public int? FileSize { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public DateTime? FileModificationDate { get; set; }
        // Add other properties that you may find in your JSON output
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeError
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Helper functions for interacting with MkvMerge.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Required for JSON deserialization>")]
    public class MkvMergeOutput
    {
        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for JSON deserialization")]
        [SuppressMessage("Microsoft.Usage", "CA1002:DoNotExposeGenericLists", Justification = "Required for JSON deserialization")]
        public List<MkvMergeAttachment>? Attachments { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for JSON deserialization")]
        [SuppressMessage("Microsoft.Usage", "CA1002:DoNotExposeGenericLists", Justification = "Required for JSON deserialization")]
        public List<MkvMergeChapter>? Chapters { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        public MkvMergeContainer? Container { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for JSON deserialization")]
        [SuppressMessage("Microsoft.Usage", "CA1002:DoNotExposeGenericLists", Justification = "Required for JSON deserialization")]
        public List<MkvMergeTrack>? Tracks { get; set; }

        /// <summary>
        /// Gets or Sets Helper functions for interacting with MkvMerge.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for JSON deserialization")]
        [SuppressMessage("Microsoft.Usage", "CA1002:DoNotExposeGenericLists", Justification = "Required for JSON deserialization")]
        public List<MkvMergeError>? Errors { get; set; }
        // Add other properties that you may find in your JSON output
    }
}
