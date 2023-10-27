using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Data.Entities.Libraries;
using Jellyfin.Plugin.Remuxer.Configuration;
using Jellyfin.Plugin.Remuxer.Models;
using Jellyfin.Plugin.Remuxer.Tools;
using Microsoft.Extensions.Primitives;

namespace Jellyfin.Plugin.Remuxer
{
    internal enum TrackType
    {
        Audio,
        Subtitle
    }

    internal class MKVProcessor
    {
        internal MKVProcessor(PluginConfiguration configuration, string mkvFilePath)
        {
            Config = configuration;
            MKVFilePath = mkvFilePath;
            WorkingDirectory = GetUniqueWorkingFolder(mkvFilePath);
            TracksToStrip = new List<Track>();
            TracksToExtract = new List<Track>();
            TracksToMerge = new List<Track>();
            SubtitlesToOCR = new List<Track>();
        }

        public PluginConfiguration Config { get; private set; }

        public string MKVFilePath { get; private set; }

        private string WorkingDirectory { get; }

        public List<Track> TracksToStrip { get; private set; }

        public List<Track> TracksToExtract { get; private set; }

        public List<Track> TracksToMerge { get; private set; }

        public List<Track> SubtitlesToOCR { get; private set; }

        public async Task ProcessAsync()
        {
            IdentifyTracks();
            HandleSubtitleExtraction();
            await HandleOCRProcessingAsync().ConfigureAwait(true);
            HandleFinalRemuxing();

            try
            {
                Directory.Delete(WorkingDirectory, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void IdentifyTracks()
        {
            // Call GetMkvInfo to get the MKV information.
            var mkvInfo = MkvMergeIdentify.GetMkvInfo(MKVFilePath);
            if (mkvInfo == null)
            {
                // Handle error - MKV info could not be retrieved.
                throw new InvalidOperationException("Could not retrieve MKV info.");
            }

            // Parse the whitelisted languages.
            var whitelistedLanguages = Config.WhitelistedLanguages.Split(',');

            // Iterate through the tracks in the MKV file.
            foreach (var mkvTrack in mkvInfo.Tracks!)
            {
                var track = new Track
                {
                    Id = mkvTrack.Id,
                    Codec = mkvTrack.Codec,
                    Language = mkvTrack.Properties?.Language,
                    TrackName = mkvTrack.Properties?.TrackName.RemoveInvalidFileNameChars(),
                    IsDefault = mkvTrack.Properties?.DefaultTrack,
                    IsForced = mkvTrack.Properties?.ForcedTrack,
                    IsOriginal = mkvTrack.Properties?.OriginalTrack
                };

                switch (mkvTrack.Type)
                {
                    case "audio":
                        track.Type = TrackType.Audio;
                        if (Config.StripMode == RemuxStripMode.StripAudio || Config.StripMode == RemuxStripMode.StripBoth)
                        {
                            if (!(whitelistedLanguages.Contains(track.Language) || (Config.KeepDefaultTrack == true && track.IsDefault == true)))
                            {
                                TracksToStrip.Add(track);
                            }
                        }

                        break;

                    case "subtitles":
                        track.Type = TrackType.Subtitle;
                        bool isTextSubtitle = track.Codec != "PGS" && track.Codec != "VobSub";
                        bool isWhitelistedLanguage = whitelistedLanguages.Contains(track.Language);
                        bool shouldExtract = Config.ExtractSubsMode != RemuxExtractMode.DoNone;
                        bool shouldOCR = Config.OCRMode != RemuxOCRMode.DoNone && (!isTextSubtitle || Config.OCRAlways);

                        if (isWhitelistedLanguage && shouldExtract)
                        {
                            if (!Config.ExtractOnlyTextSubs || isTextSubtitle)
                            {
                                TracksToExtract.Add(track);
                            }
                        }

                        if (isWhitelistedLanguage && shouldOCR)
                        {
                            SubtitlesToOCR.Add(track);
                        }

                        break;

                    default:
                        // Handle other track types if necessary.
                        break;
                }
            }

            var fileDir = Path.GetDirectoryName(MKVFilePath)!;
            var fileName = Path.GetFileNameWithoutExtension(MKVFilePath);
            var existingFiles = Directory.GetFiles(fileDir, $"{fileName}.*");

            foreach (var file in existingFiles)
            {
                var parts = Path.GetFileName(file).Split('.');

                var trackId = int.Parse(parts[1], CultureInfo.InvariantCulture);
                var trackLang = parts[2];
                var trackName = parts[3];
                var fileExtension = parts.Last();

                // Assume that the track ID in the file name corresponds to a track ID in the MKV file.
                var track = new Track
                {
                    Id = trackId,
                    Language = trackLang,
                    TrackName = trackName.Replace(' ', '_'),  // Replace spaces back to underscores.
                    FilePath = file,
                };

                // Determine the track type based on the file extension.
                switch (fileExtension)
                {
                    case "srt":
                    case "ass":
                    case "ssa":
                        track.Type = TrackType.Subtitle;
                        track.Codec = "Text";
                        break;
                    case "sub":
                        track.Type = TrackType.Subtitle;
                        track.Codec = "PGS";
                        break;
                    case "sup":
                        track.Type = TrackType.Subtitle;
                        track.Codec = "VobSub";
                        break;
                    default:
                        continue;  // Skip if the file extension is not recognized.
                }

                // Check whether to OCR this subtitle based on its type and the configuration.
                bool isTextSubtitle = track.Codec == "Text";
                bool isWhitelistedLanguage = Config.WhitelistedLanguages.Split(',').Contains(track.Language);
                bool shouldOCR = Config.OCRMode != RemuxOCRMode.DoNone && (!isTextSubtitle || Config.OCRAlways);

                if (isWhitelistedLanguage && shouldOCR)
                {
                    SubtitlesToOCR.Add(track);
                }
            }
        }

        private void HandleSubtitleExtraction()
        {
            if (TracksToExtract.Count == 0)
            {
                return;
            }

            // Build the arguments for mkvextract
            var argsBuilder = $@"""{MKVFilePath}"" tracks";

            var fileDir = WorkingDirectory;
            var fileName = Path.GetFileNameWithoutExtension(MKVFilePath);

            foreach (var track in TracksToExtract)
            {
                string trackFilePath = Path.Join(fileDir, $"{fileName}.{track.Id}.{track.Language}.{track.TrackName}.{Utils.GetFileExtFromCodec(track.Codec!)}");
                track.FilePath = trackFilePath;

                argsBuilder += $" {track.Id}:{trackFilePath}";
            }

            foreach (var track in SubtitlesToOCR)
            {
                if ((!TracksToExtract.Any(t => t.Id == track.Id)) && track.FilePath == null)
                {
                    var trackFilePath = Path.Join(fileDir, $"{fileName}.{track.Id}.{track.Language}.{track.TrackName}.{Utils.GetFileExtFromCodec(track.Codec!)}");
                    argsBuilder += $" {track.Id}:{trackFilePath}";
                    track.FilePath = trackFilePath;
                }
            }

            string args = argsBuilder;

            // Set up the process start info for mkvextract
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "mkvextract",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // Start the process and wait for it to exit
            using var process = Process.Start(startInfo);
            process!.WaitForExit();

            // Check the exit code
            if (process.ExitCode != 0)
            {
                // Log standard output and standard error if the exit code is not 0
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                // Assume Log is a method to log messages to your logging system
                Console.WriteLine($"mkvextract failed with exit code {process.ExitCode}. Standard Output: {stdout}. Standard Error: {stderr}");
            }
        }

        private async Task HandleOCRProcessingAsync()
        {
            var ocrTasks = SubtitlesToOCR.Select(track => OCRSubtitleTrack(track));
            await Task.WhenAll(ocrTasks).ConfigureAwait(true);
        }

        private async Task OCRSubtitleTrack(Track subtitleTrack)
        {
            // Define the command and arguments for SubtitleEdit
            string args = $@"/convert ""{subtitleTrack.FilePath}"" subrip";  // Assume the subtitle file path is in FilePath property

            // Set up the process start info for SubtitleEdit
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "subtitleedit",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // Start the OCR process
            using var process = Process.Start(startInfo);

            // Await the completion of the OCR process
            await process!.WaitForExitAsync().ConfigureAwait(true);

            // Check the exit code to see if the OCR was successful
            if (process.ExitCode == 0)
            {
                // Assume the OCR'ed file will have the same name but with an .srt extension
                string ocrFilePath = Path.ChangeExtension(subtitleTrack.FilePath!, ".srt");

                // Verify the OCR'ed file exists
                if (File.Exists(ocrFilePath))
                {
                    // Update the FilePath property of the subtitleTrack
                    subtitleTrack.FilePath = ocrFilePath;

                    // Check the configuration to see if OCR'ed tracks should be merged
                    if (Config.ExtractSubsMode == RemuxExtractMode.DoNone)
                    {
                        TracksToMerge.Add(subtitleTrack);
                    }
                }
                else
                {
                    // Handle the case where the OCR'ed file does not exist
                    Console.WriteLine($"OCR'ed file does not exist: {ocrFilePath}");
                }
            }
            else
            {
                // Log standard output and standard error if the exit code is not 0
                string stdout = await process!.StandardOutput.ReadToEndAsync().ConfigureAwait(true);
                string stderr = await process!.StandardError.ReadToEndAsync().ConfigureAwait(true);

                Console.WriteLine($"SubtitleEdit failed with exit code {process.ExitCode}. Standard Output: {stdout}. Standard Error: {stderr}");
            }
        }

        private void HandleFinalRemuxing()
        {
            // Building the mkvmerge command
            var outputFile = Path.Join(WorkingDirectory, Path.GetFileName(MKVFilePath));
            var commandBuilder = string.Empty;

            commandBuilder += $@"-o ""{outputFile}"" ";  // Specifies the output file

            // Handling audio track stripping
            if (TracksToStrip.Any(t => t.Type == TrackType.Audio))
            {
                string audioIDs = string.Join(',', TracksToStrip.Where(t => t.Type == TrackType.Audio).Select(t => t.Id));
                commandBuilder += $"-a !{audioIDs} ";
            }

            // Handling subtitle track stripping
            if (TracksToStrip.Any(t => t.Type == TrackType.Subtitle))
            {
                string subtitleIDs = string.Join(',', TracksToStrip.Where(t => t.Type == TrackType.Subtitle).Select(t => t.Id));
                commandBuilder += $"-s !{subtitleIDs} ";
            }

            commandBuilder += $@"""{MKVFilePath}"" ";      // Specifies the input file

            // Handling subtitle track merging
            foreach (var track in TracksToMerge)
            {
                commandBuilder += $@"""{track.FilePath}"" ";
            }

            string args = commandBuilder;

            // Setting up the process start info for mkvmerge
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "mkvmerge",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // Starting the mkvmerge process
            using var process = Process.Start(startInfo);

            // Awaiting the completion of the mkvmerge process
            process!.WaitForExit();

            // Checking the exit code to see if the remuxing was successful
            if (process.ExitCode != 0)
            {
                // Log standard output and standard error if the exit code is not 0
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                Console.WriteLine($"mkvmerge failed with exit code {process.ExitCode}. Standard Output: {stdout}. Standard Error: {stderr}");
            }
            else
            {
                File.Delete(MKVFilePath);
                File.Copy(outputFile, MKVFilePath);
                File.Delete(outputFile);

                foreach (var track in SubtitlesToOCR)
                {
                    if (Path.GetDirectoryName(track.FilePath) == WorkingDirectory && (!TracksToMerge.Contains(track)))
                    {
                        File.Copy(track.FilePath!, Path.Join(Path.GetDirectoryName(MKVFilePath), Path.GetFileName(track.FilePath)));
                        File.Delete(track.FilePath!);
                    }
                    else if (TracksToMerge.Contains(track))
                    {
                        File.Delete(track.FilePath!);
                    }
                }
            }
        }

        private string GetUniqueWorkingFolder(string mkvFilePath)
        {
            // Get the directory of the MKV file
            string directory = Path.GetDirectoryName(mkvFilePath)!;

            // Get the name of the MKV file without the extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mkvFilePath);

            // Create a hash of the file name
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(fileNameWithoutExtension));
                StringBuilder hashBuilder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    hashBuilder.Append(bytes[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                string hash = hashBuilder.ToString();

                // Combine the directory, file name, and hash to form the unique working folder path
                string uniqueWorkingFolder = Path.Combine(directory, $"{fileNameWithoutExtension}_{hash}");

                // Ensure the working folder exists
                if (!Directory.Exists(uniqueWorkingFolder))
                {
                    Directory.CreateDirectory(uniqueWorkingFolder);
                }

                return uniqueWorkingFolder;
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "<Is relavent>")]
    internal class Track
    {
        public int Id { get; set; }

        public TrackType Type { get; set; }

        public string? Codec { get; set; }

        public string? Language { get; set; }

        public string? TrackName { get; set; }

        public string? FilePath { get; set; } // Path to the extracted subtitle file on disk

        public bool? IsDefault { get; set; }

        public bool? IsForced { get; set; }

        public bool? IsOriginal { get; set; }
    }
}
