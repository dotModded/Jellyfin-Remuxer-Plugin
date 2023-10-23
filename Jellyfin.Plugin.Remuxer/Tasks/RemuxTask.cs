using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.Remuxer.Tools;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Remuxer.Tasks;

/// <summary>
/// Scheduled task to remux media for immediate access in web player.
/// </summary>
public class RemuxTask : IScheduledTask
{
    private const int QueryPageLimit = 100;

    private readonly ILibraryManager _libraryManager;
    private readonly ISubtitleEncoder _subtitleEncoder;
    private readonly ILocalizationManager _localization;

    private static readonly BaseItemKind[] _itemTypes = { BaseItemKind.Episode, BaseItemKind.Movie };
    private static readonly string[] _mediaTypes = { MediaType.Video };
    private static readonly SourceType[] _sourceTypes = { SourceType.Library };

    /// <summary>
    /// Initializes a new instance of the <see cref="RemuxTask" /> class.
    /// </summary>
    /// <param name="libraryManager">Instance of <see cref="ILibraryManager"/> interface.</param>
    /// /// <param name="subtitleEncoder">Instance of <see cref="ISubtitleEncoder"/> interface.</param>
    /// <param name="localization">Instance of <see cref="ILocalizationManager"/> interface.</param>
    /// <param name="loggerFactory">Instance of the <see cref="ILoggerFactory"/> interface.</param>
    public RemuxTask(
        ILibraryManager libraryManager,
        ISubtitleEncoder subtitleEncoder,
        ILocalizationManager localization,
        ILoggerFactory loggerFactory)
    {
        _libraryManager = libraryManager;
        _subtitleEncoder = subtitleEncoder;
        _localization = localization;
    }

    /// <inheritdoc />
    public string Name => Plugin.Instance!.Name;

    /// <inheritdoc />
    public string Key => "Remux";

    /// <inheritdoc />
    public string Description => Plugin.Instance!.Description;

    /// <inheritdoc />
    public string Category => _localization.GetLocalizedString("TasksLibraryCategory");

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var query = new InternalItemsQuery
        {
            Recursive = true,
            HasSubtitles = true,
            IsVirtualItem = false,
            IncludeItemTypes = _itemTypes,
            MediaTypes = _mediaTypes,
            SourceTypes = _sourceTypes,
            Limit = QueryPageLimit,
        };

        var numberOfVideos = _libraryManager.GetCount(query);

        var startIndex = 0;
        var completedVideos = 0;

        while (startIndex < numberOfVideos)
        {
            query.StartIndex = startIndex;
            var videos = _libraryManager.GetItemList(query);

            foreach (var video in videos)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (video.Container)
                {
                    case string s when s.Contains("mkv", StringComparison.OrdinalIgnoreCase):
                        MKVRemux.ProcessMediaItem(video);
                        break;
                    default:
                        break;
                }

                completedVideos++;
                progress.Report(100d * completedVideos / numberOfVideos);
            }

            startIndex += QueryPageLimit;
        }

        progress.Report(100);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => Enumerable.Empty<TaskTriggerInfo>();
}
