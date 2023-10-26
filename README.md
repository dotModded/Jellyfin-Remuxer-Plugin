# Jellyfin Remuxer Plugin

This plugin is designed to remux all media in your library. Fixing common streaming problems, saving space by stripping unwanted languages from subtitles or audio, and other utilities for managing your media. Utilizes MKVToolNix for manipulation of Matroska files, with more planned in the future. Heads up I'm a graphics developer by trade fhough C# is new to me, apologies for messy code I'm still learning. This project is mostly for personal use but it may prove useful to someone, I reccomend listening to the warning below.   

### ***WARNING*** This plugin can changes media files perminantly and currently cannot guarentee recovery from failure. It is highly a work in progress, this repo is for development purposes only at this time.

## Requirements

As this project is for development purposes, no precompiled binaries are provided. All requirements must be available in your Path.

- [Dotnet SDK 6.0](https://dotnet.microsoft.com/download)

- [MKVToolNix](https://mkvtoolnix.download/downloads.html)

- [SubtitleEdit](https://github.com/SubtitleEdit/subtitleedit/releases)

## Currently Developing

- Automatic OCR Image Subs with Subtitle Edit

- Improved tmp folder handling

## Future Plans

- Better language code handling.

- Cache MKV identify in database and skip already processed files.

- Finish MKV identify object raw representation, and create generic track info types.

- Create more robust settings for finer control of remux. Especially with subtitles.

- Async with configurable process count.

- Process newly added media to Jellyfin.

- UI for controlling settings per video or collections thereof. Potential Issue is colliding settings.

- MP4 and DolbyMP4 Pipelines.

- Dolby Vision Profile Conversions.