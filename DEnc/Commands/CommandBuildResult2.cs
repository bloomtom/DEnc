using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEnc.Commands
{
    internal class CommandBuildResult2
    {
        internal CommandBuildResult2(string commandArguments, IEnumerable<StreamVideoFile> videoPieces, IEnumerable<StreamAudioFile> audioPieces, IEnumerable<StreamSubtitleFile> subtitlePieces)
        {
            RenderedCommand = commandArguments;
            VideoPieces = videoPieces;
            AudioPieces = audioPieces ?? new List<StreamAudioFile>();
            SubtitlePieces = subtitlePieces ?? new List<StreamSubtitleFile>(); ;
        }

        public string RenderedCommand { get; private set; }
        public IEnumerable<StreamVideoFile> VideoPieces { get; private set; }
        public IEnumerable<StreamAudioFile> AudioPieces { get; private set; }
        public IEnumerable<StreamSubtitleFile> SubtitlePieces { get; private set; }

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
    }
}
