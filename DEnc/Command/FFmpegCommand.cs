using DEnc.Models;
using System.Collections.Generic;
using System.Linq;

namespace DEnc.Commands
{
    /// <summary>
    /// Encapsulates all the needed parts to produce an ffmpeg command to encode a file for DASH.
    /// </summary>
    public class FFmpegCommand
    {
        ///<inheritdoc cref="FFmpegCommand"/>
        public FFmpegCommand(IEnumerable<string> topLevelCommands, IEnumerable<VideoStreamCommand> videoPieces, IEnumerable<AudioStreamCommand> audioPieces, IEnumerable<SubtitleStreamCommand> subtitlePieces)
        {
            TopLevelCommands = topLevelCommands;
            VideoCommands = videoPieces;
            AudioCommands = audioPieces ?? new List<AudioStreamCommand>();
            SubtitleCommands = subtitlePieces ?? new List<SubtitleStreamCommand>();
        }

        /// <summary>
        /// Returns the combined Video, Audio, and Subtitle <see cref="IStreamCommand"/> stream commands.
        /// </summary>
        public IEnumerable<IStreamCommand> AllStreamCommands => VideoCommands.Union<IStreamCommand>(AudioCommands).Union(SubtitleCommands);

        /// <summary>
        /// A collection of all the audio stream commands included in this ffmpeg command.
        /// </summary>
        public IEnumerable<AudioStreamCommand> AudioCommands { get; private set; }

        /// <summary>
        /// The complete, executable ffmpeg command.
        /// </summary>
        public string RenderedCommand => string.Join("\t", TopLevelCommands.Union(AllStreamCommands.Select(x => x.Argument)));

        /// <summary>
        /// A collection of all the subtitle stream commands included in this ffmpeg command.
        /// </summary>
        public IEnumerable<SubtitleStreamCommand> SubtitleCommands { get; private set; }

        /// <summary>
        /// The top level commands to include in the rendered ffmpeg command.
        /// </summary>
        public IEnumerable<string> TopLevelCommands { get; private set; }

        /// <summary>
        /// A collection of all the video stream commands included in this ffmpeg command.
        /// </summary>
        public IEnumerable<VideoStreamCommand> VideoCommands { get; private set; }
    }
}