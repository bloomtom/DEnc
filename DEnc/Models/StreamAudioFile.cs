namespace DEnc.Models
{
    /// <summary>
    /// The definition for how to produce an audio stream during encoding.
    /// </summary>
    public class AudioStreamCommand : IStreamCommand
    {
        ///<inheritdoc cref="IStreamCommand.Argument"/>
        public string Argument { get; set; }
        ///<inheritdoc cref="IStreamCommand.Index"/>
        public int Index { get; set; }
        /// <summary>
        /// The name for this audio stream containing a language and title.
        /// </summary>
        public string Name { get; set; }
        ///<inheritdoc cref="IStreamCommand.Path"/>
        public string Path { get; set; }
        /// <summary>
        /// This is an audio stream.
        /// </summary>
        public StreamType Type => StreamType.Audio;
    }
}