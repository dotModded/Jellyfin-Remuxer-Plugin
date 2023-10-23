using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Remuxer.Tools
{
    internal class MKVRemux
    {
        public MKVRemux()
        {
        }

        public static void ProcessMediaItem(BaseItem video)
        {
            if (video.Path == null)
            {
                return;
            }

            var mkvInfo = MkvMergeHelper.GetMkvInfo(video.Path);

            if (mkvInfo != null && mkvInfo.Tracks != null)
            {
                StripMedia(video, ref mkvInfo);
                ExtractSubtitles(video, mkvInfo);
                OCRSubtitles(video, mkvInfo);
            }
        }

        public static void StripMedia(BaseItem video, ref MkvMergeOutput mkvInfo)
        {
            var config = Plugin.Instance!.Configuration;
            var languages = config.WhitelistedLanguages.Split(',');
            HashSet<string> allowedLanguages = new HashSet<string>(languages);

            var stripAudio = config.StripMode == Configuration.RemuxStripMode.StripAudio || config.StripMode == Configuration.RemuxStripMode.StripBoth;
            var stripSubs = config.StripMode == Configuration.RemuxStripMode.StripSubtitles || config.StripMode == Configuration.RemuxStripMode.StripBoth;

            var removeAudioList = new List<string>();
            var removeSubtitleList = new List<string>();
            mkvInfo.Tracks!.ForEach(track =>
            {
                if ((track.Type != "audio" || !stripAudio) && (track.Type != "subtitles" || !stripSubs))
                {
                    return;
                }

                if (config.KeepDefaultTrack && (track.Properties!.DefaultTrack == true || track.Properties!.ForcedTrack == true))
                {
                    return;
                }

                if (Utils.IsLanguageAllowed(track.Properties!.Language!, allowedLanguages))
                {
                    return;
                }

                if (track.Type == "audio")
                {
                    removeAudioList.Add($"{track.Id}");
                }
                else
                {
                    removeSubtitleList.Add($"{track.Id}");
                }
            });

            if (removeAudioList.Count > 0 || removeSubtitleList.Count > 0)
            {
                var tmpFolder = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(video.Path))!, "tmp");
                var outputFile = Path.Combine(tmpFolder, Path.GetFileName(video.Path));
                var stripAudioString = $"-a !{string.Join(",", removeAudioList)}";
                var stripSubtitleString = $"-s !{string.Join(",", removeSubtitleList)}";

                var args = $@"-o ""{outputFile}""";

                if (removeAudioList.Count > 0)
                {
                    args += " " + stripAudioString;
                }

                if (removeSubtitleList.Count > 0)
                {
                    args += " " + stripSubtitleString;
                }

                args += $@" ""{video.Path}""";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "mkvmerge",
                    Arguments = args,
                    RedirectStandardOutput = true, // Redirects stdout so we can read it directly
                    UseShellExecute = false, // Necessary to redirect IO
                    CreateNoWindow = true // Prevents a command window from popping up
                };

                using var process = Process.Start(startInfo);
                using StreamReader reader = process!.StandardOutput;
                string mkvMergeOutput = reader.ReadToEnd();

                if (process.ExitCode == 0 && mkvMergeOutput.Contains("multiplexing took", StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(video.Path);
                    File.Copy(outputFile, video.Path);
                    File.Delete(outputFile);
                    Directory.Delete(tmpFolder);
                }

                mkvInfo = MkvMergeHelper.GetMkvInfo(video.Path)!;
            }
        }

        public static void ExtractSubtitles(BaseItem video, MkvMergeOutput mkvInfo)
        {
        }

        public static void OCRSubtitles(BaseItem video, MkvMergeOutput mkvInfo)
        {
        }
    }
}
