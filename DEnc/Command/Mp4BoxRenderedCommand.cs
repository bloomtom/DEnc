namespace DEnc.Commands
{
    /// <summary>
    /// Container used to house the rendered Mp4Box command and its output path
    /// </summary>
    public class Mp4BoxRenderedCommand
    {
        ///<inheritdoc cref="Mp4BoxRenderedCommand"/>
        public Mp4BoxRenderedCommand(string renderedCommand, string mpdPath)
        {
            RenderedCommand = renderedCommand;
            MpdPath = mpdPath;
        }

        /// <summary>
        /// The disk path for the output MPD file.
        /// </summary>
        public string MpdPath { get; private set; }
        /// <summary>
        /// The rendered command to pass to MP4Box
        /// </summary>
        public string RenderedCommand { get; private set; }
    }
}