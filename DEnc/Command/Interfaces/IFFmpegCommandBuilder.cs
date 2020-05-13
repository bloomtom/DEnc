using DEnc.Models.Interfaces;
using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands.Interfaces
{
    internal interface IFFmpegCommandBuilder
    {
        FfmpegRenderedCommand Build();
        IFFmpegCommandBuilder WithVideoCommands(IEnumerable<MediaStream> videoStreams, IEnumerable<IQuality> qualities, int framerate, int keyframeInterval, int defaultBitrate);
        IFFmpegCommandBuilder WithAudioCommands(IEnumerable<MediaStream> streams);
        IFFmpegCommandBuilder WithSubtitleCommands(IEnumerable<MediaStream> streams);
    }
}
