using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DEnc
{
    /// <summary>
    /// A representation of the output MPEG DASH mpd file.
    /// </summary>
    public class DashEncodeResult
    {
        /// <summary>
        /// Create a typical instance of DashEncodeResult.
        /// </summary>
        /// <param name="file">The exact mpd content deserialized from XML.</param>
        /// <param name="metadata">Arbitrary metadata about the output streams.</param>
        /// <param name="duration">The play duration of the media.</param>
        /// <param name="path">The exact path to the mpd file.</param>
        public DashEncodeResult(MPD file, IReadOnlyDictionary<string, string> metadata, TimeSpan duration, string path)
        {
            DashFileContent = file;
            Metadata = metadata;
            FileDuration = duration;
            DashFilePath = path;
        }

        /// <summary>
        /// This is the exact mpd content deserialized from XML.
        /// </summary>
        public MPD DashFileContent { get; protected set; }

        /// <summary>
        /// The exact path to the mpd file.
        /// </summary>
        public string DashFilePath { get; protected set; }

        /// <summary>
        /// The play duration of the media.
        /// </summary>
        public TimeSpan FileDuration { get; protected set; }

        /// <summary>
        /// Returns the list of media filenames from the DashFileContent. This operation scans the MPD object and isn't cached. Does not return filenames when a live profile is used.
        /// </summary>
        public IEnumerable<string> MediaFiles => DashFileContent?.Period.SelectMany(x => x.AdaptationSet.SelectMany(y => y.Representation.SelectMany(z => z.BaseURL)));

        /// <summary>
        /// Arbitrary metadata about the output streams.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }
    }
}