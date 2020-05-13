using DEnc.Models;
using DEnc.Models.Interfaces;
using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands.Interfaces
{
    internal interface IFFmpegVideoCommandBuilder
    {
        StreamVideoFile Build();
        IFFmpegVideoCommandBuilder WithSize(IQuality quality);
        IFFmpegVideoCommandBuilder WithBitrate(int bitrate);
        IFFmpegVideoCommandBuilder WithBitrate(int bitrate, int defaultBitrate);
        IFFmpegVideoCommandBuilder WithPreset(H264Preset preset);
        IFFmpegVideoCommandBuilder WithProfile(H264Profile profile);
        IFFmpegVideoCommandBuilder WithProfileLevel(string level);
        IFFmpegVideoCommandBuilder WithPixelFormat(string format);
        IFFmpegVideoCommandBuilder WithFramerate(int framerate);
        IFFmpegVideoCommandBuilder WithVideoCodec(string sourceCodec, int keyInterval, bool enableCopy);
    }
}
