using DEnc.Models;
using DEnc.Models.Interfaces;
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
        public FFmpegCommand(string commandArguments, IEnumerable<StreamVideoFile> videoPieces, IEnumerable<StreamAudioFile> audioPieces, IEnumerable<StreamSubtitleFile> subtitlePieces)
        {
            RenderedCommand = commandArguments;
            VideoPieces = videoPieces;
            AudioPieces = audioPieces ?? new List<StreamAudioFile>();
            SubtitlePieces = subtitlePieces ?? new List<StreamSubtitleFile>();
        }

        /// <summary>
        /// Returns the combined Video, Audio, and Subtitle <see cref="IStreamFile"/> pieces.
        /// </summary>
        public IEnumerable<IStreamFile> AllPieces
        {
            get
            {
                IEnumerable<IStreamFile>[] pieces = new IEnumerable<IStreamFile>[]
                {
                    VideoPieces,
                    AudioPieces,
                    SubtitlePieces
                };
                return pieces.SelectMany(x => x);
            }
        }

        /// <summary>
        /// A collection of all the audio streams included in this ffmpeg command.
        /// </summary>
        public IEnumerable<StreamAudioFile> AudioPieces { get; private set; }
        /// <summary>
        /// The complete, executable ffmpeg command.
        /// </summary>
        public string RenderedCommand { get; private set; }
        /// <summary>
        /// A collection of all the subtitle streams included in this ffmpeg command.
        /// </summary>
        public IEnumerable<StreamSubtitleFile> SubtitlePieces { get; private set; }
        /// <summary>
        /// A collection of all the video streams included in this ffmpeg command.
        /// </summary>
        public IEnumerable<StreamVideoFile> VideoPieces { get; private set; }
    }
}