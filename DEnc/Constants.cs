using DEnc.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEnc
{

    internal static class Constants
    {
        internal static IReadOnlyDictionary<string, Codec> SupportedCodecs { get; } = new Dictionary<string, Codec>()
        {
            ["opus"] = new Codec("opus", "ogg", "ogg"),
            ["aac"] = new Codec("aac", "mp4", "aac"),
            ["mp3"] = new Codec("mp3", "mp3", "mp3"),
            ["h264"] = new Codec("h264", "mp4", "mp4"),
            ["vp8"] = new Codec("vp8", "webm", "webm")
        };

        internal static HashSet<string> SupportedSubtitleCodecs { get; } = new HashSet<string>()
        {
            "webvtt",
            "ass",
            "mov_text",
            "subrip",
            "text"
        };

        internal static HashSet<string> IllegalFilesystemChars = new HashSet<string>
        {
            "#", "&", "*", "<", ">", "/", "?", ":", "\"", "%"
        };
    }
}
