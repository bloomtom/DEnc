using DEnc.Models;
using DEnc.Models.Interfaces;
using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands.Interfaces
{
    internal interface IFFmpegAudioCommandBuilder
    {
        StreamAudioFile Build();
        IFFmpegAudioCommandBuilder WithLanguage();
        IFFmpegAudioCommandBuilder WithTitle();
        IFFmpegAudioCommandBuilder WithCodec();
    }
}
