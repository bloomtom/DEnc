using DEnc.Models;
using DEnc.Models.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DEnc.Commands
{
    internal class FfmpegRenderedCommand
    {
        internal FfmpegRenderedCommand(string commandArguments, IEnumerable<StreamVideoFile> videoPieces, IEnumerable<StreamAudioFile> audioPieces, IEnumerable<StreamSubtitleFile> subtitlePieces)
        {
            RenderedCommand = commandArguments;
            VideoPieces = videoPieces;
            AudioPieces = audioPieces ?? new List<StreamAudioFile>();
            SubtitlePieces = subtitlePieces ?? new List<StreamSubtitleFile>(); ;
        }

        /// <summary>
        /// Returns the combined Video, Audio, and Subtitle <see cref="IStreamFile"/> pieces
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

        public IEnumerable<StreamAudioFile> AudioPieces { get; private set; }
        public string RenderedCommand { get; private set; }
        public IEnumerable<StreamSubtitleFile> SubtitlePieces { get; private set; }
        public IEnumerable<StreamVideoFile> VideoPieces { get; private set; }
    }
}