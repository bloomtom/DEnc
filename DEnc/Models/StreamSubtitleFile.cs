namespace DEnc.Models
{
    /// <summary>
    /// The definition for how to produce a subtitle stream during encoding.
    /// </summary>
    public class SubtitleStreamCommand : IStreamCommand
    {
        ///<inheritdoc cref="IStreamCommand.Argument"/>
        public string Argument { get; set; }
        ///<inheritdoc cref="IStreamCommand.Index"/>
        public int Index { get; set; }
        /// <summary>
        /// The language code for these subtitles.
        /// </summary>
        public string Language { get; set; }
        ///<inheritdoc cref="IStreamCommand.Path"/>
        public string Path { get; set; }
        /// <summary>
        /// This is a subtitle stream.
        /// </summary>
        public StreamType Type => StreamType.Subtitle;
    }
}