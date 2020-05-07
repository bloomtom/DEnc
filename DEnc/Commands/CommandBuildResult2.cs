using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands
{
    internal class CommandBuildResult2
    {
        internal CommandBuildResult2(string commandArguments, IEnumerable<StreamVideoFile> videoPieces, IEnumerable<StreamAudioFile> audioPieces, IEnumerable<StreamSubtitleFile> subtitlePieces)
        {
            RenderedCommand = commandArguments;
            VideoPieces = videoPieces;
            AudioPieces = audioPieces;
            SubtitlePieces = subtitlePieces;
        }

        public string RenderedCommand { get; private set; }
        public IEnumerable<StreamVideoFile> VideoPieces { get; private set; }
        public IEnumerable<StreamAudioFile> AudioPieces { get; private set; }
        public IEnumerable<StreamSubtitleFile> SubtitlePieces { get; private set; }
    }
}
