using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Remuxer.Configuration;

/// <summary>
/// Configuration defining how to handle stripping tracks from media.
/// </summary>
public enum RemuxStripMode
{
    /// <summary>
    /// Do not touch files.
    /// </summary>
    DoNone,

    /// <summary>
    /// Remove unwanted Subtitle Tracks from all media.
    /// </summary>
    StripSubtitles,

    /// <summary>
    /// Remove unwanted Audio Tracks from all media.
    /// </summary>
    StripAudio,

    /// <summary>
    /// Remove unwanted Audio and Subtitle Tracks from all media.
    /// </summary>
    StripBoth,
}

/// <summary>
/// Configuration defining how to handle extracting subtitle tracks from media.
/// </summary>
public enum RemuxExtractMode
{
    /// <summary>
    /// Do not extract subs.
    /// </summary>
    DoNone,

    /// <summary>
    /// Extracts Subtitle Tracks, preserving the subtitles in the container.
    /// </summary>
    ExtractOnly,

    /// <summary>
    /// Extracts Subtitle Tracks, deleting the subtitles in the container.
    /// </summary>
    ExtractAndRemux,
}

/// <summary>
/// Configuration defining how to OCR image based subs.
/// </summary>
public enum RemuxOCRMode
{
    /// <summary>
    /// Do not OCR files.
    /// </summary>
    DoNone,

    /// <summary>
    /// Utilizes Tesseract to OCR image based subtitles.
    /// </summary>
    Tesseract,

    /// <summary>
    /// Utilizes nOCR to OCR image based subtitles.
    /// </summary>
    NOCR,
}

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        WhitelistedLanguages = "eng";
        KeepDefaultTrack = true;
        StripMode = RemuxStripMode.DoNone;
        ExtractSubsMode = RemuxExtractMode.DoNone;
        ExtractOnlyTextSubs = true;
        OCRMode = RemuxOCRMode.DoNone;
        // Defauult behavior wont ocr languages that already have text subs.
        OCRAlways = false;
    }

    /// <summary>
    /// Gets or sets the languages to keep during remuxing.
    /// </summary>
    public string WhitelistedLanguages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to keep the default audio and subtitle tracks.
    /// </summary>
    public bool KeepDefaultTrack { get; set;  }

    /// <summary>
    /// Gets or sets the mode to use when stripping subtitle and audio tracks from the media container.
    /// </summary>
    public RemuxStripMode StripMode { get; set; }

    /// <summary>
    /// Gets or sets the mode to use when extracting subtitles.
    /// </summary>
    public RemuxExtractMode ExtractSubsMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to extract image subtitle tracks.
    /// </summary>
    public bool ExtractOnlyTextSubs { get; set; }

    /// <summary>
    /// Gets or sets the mode to use when using OCR on image subtitles.
    /// </summary>
    public RemuxOCRMode OCRMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to OCR Image Subtitles even if text subtitles are available.
    /// </summary>
    public bool OCRAlways { get; set; }
}
