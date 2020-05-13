using DEnc.Models;
using DEnc.Models.Interfaces;
using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DEnc.Commands
{
    internal class FFmpegAudioCommandBuilder
    {
        List<string> commands;

        MediaStream audioStream;
        string outputDirectory;
        string outputBaseFilename;
        bool codecSupported;

        string path;
        string language;
        string title;

        public static FFmpegAudioCommandBuilder Initilize(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            FFmpegAudioCommandBuilder builder = new FFmpegAudioCommandBuilder(audioStream, outputDirectory, outputBaseFilename, additionalFlags);
            return builder;
        }

        private FFmpegAudioCommandBuilder(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            this.audioStream = audioStream;
            this.outputDirectory = outputDirectory;
            this.outputBaseFilename = outputBaseFilename;
            commands = new List<string>();

            codecSupported = Constants.SupportedCodecs.ContainsKey(audioStream.codec_name);

            commands.Add($"-map 0:{audioStream.index}");
            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
        }

        public StreamAudioFile Build()
        {
            path = Path.Combine(outputDirectory, $"{outputBaseFilename}_audio_{language}_{audioStream.index}.mp4");
            commands.Add($"\"{path}\"");

            return new StreamAudioFile
            {
                Type = StreamType.Audio,
                Index = audioStream.index,
                Name = $"{language} {title}",
                Path = path,
                Argument = string.Join(" ", commands)
            };
        }

        public FFmpegAudioCommandBuilder WithLanguage()
        {
            string language = audioStream.tag
                .Where(x => x.key == "language")
                .Select(x => x.value)
                .FirstOrDefault();

            if (language is null)
            {
                language = audioStream.disposition.@default > 0 ? "default" : "und";
            }
            this.language = language;
            return this;
        }

        public FFmpegAudioCommandBuilder WithTitle()
        {
            string title = audioStream.tag
                .Where(x => x.key == "title")
                .Select(x => x.value)
                .FirstOrDefault();

            if (title is null)
            {
                title = audioStream.index.ToString();
            }
            this.title = title;
            return this;
        }

        public FFmpegAudioCommandBuilder WithCodec()
        {
            if (codecSupported)
            {
                return this;
            }
            commands.Add($"-c:a aac -b:a {audioStream.bit_rate * 1.1}");
            return this;
        }
    }

}
