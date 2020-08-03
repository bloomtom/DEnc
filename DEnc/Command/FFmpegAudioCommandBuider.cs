using DEnc.Models;
using DEnc.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEnc.Commands
{
    internal class FFmpegAudioCommandBuilder
    {
        private readonly MediaStream audioStream;
        private readonly bool codecSupported;
        private readonly List<string> commands;
        private readonly string outputBaseFilename;
        private readonly string outputDirectory;
        private string language;
        private string path;
        private string title;

        private FFmpegAudioCommandBuilder(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            this.audioStream = audioStream;
            this.outputDirectory = outputDirectory;
            this.outputBaseFilename = outputBaseFilename;
            commands = new List<string>();

            codecSupported = Constants.SupportedOutputCodecs.ContainsKey(audioStream.codec_name);

            commands.Add($"-map 0:{audioStream.index}");
            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
        }

        public static FFmpegAudioCommandBuilder Initilize(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            FFmpegAudioCommandBuilder builder = new FFmpegAudioCommandBuilder(audioStream, outputDirectory, outputBaseFilename, additionalFlags);
            return builder;
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

        public FFmpegAudioCommandBuilder WithCodec()
        {
            if (codecSupported)
            {
                return this;
            }
            commands.Add($"-c:a aac -b:a {audioStream.bit_rate * 1.1}");
            return this;
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
    }
}