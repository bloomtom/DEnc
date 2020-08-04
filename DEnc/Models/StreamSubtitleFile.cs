using DEnc.Models.Interfaces;

namespace DEnc.Models
{
    /// <summary>
    /// The definition for how to produce a subtitle stream during encoding.
    /// </summary>
    public class StreamSubtitleFile : IStreamFile
    {
        ///<inheritdoc cref="IStreamFile.Argument"/>
        public string Argument { get; set; }
        ///<inheritdoc cref="IStreamFile.Index"/>
        public int Index { get; set; }
        /// <summary>
        /// The language code for these subtitles.
        /// </summary>
        public string Language { get; set; }
        ///<inheritdoc cref="IStreamFile.Path"/>
        public string Path { get; set; }
        /// <summary>
        /// This is a subtitle stream.
        /// </summary>
        public StreamType Type => StreamType.Subtitle;
    }
}