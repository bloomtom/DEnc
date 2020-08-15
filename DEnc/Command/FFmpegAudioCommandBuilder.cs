using DEnc.Models;
using DEnc.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEnc.Commands
{
    /// <summary>
    /// Provides a set of methods for building an ffmpeg command for a single audio output stream using AAC encoding.
    /// </summary>
    public class FFmpegAudioCommandBuilder
    {
        private readonly MediaStream audioStream;
        private readonly bool codecSupported;
        private readonly List<string> commands;
        private readonly string outputBaseFilename;
        private readonly string outputDirectory;
        private string language;
        private string path;
        private string title;

        /// <inheritdoc cref="FFmpegAudioCommandBuilder"/>
        /// <param name="audioStream">The audio stream to derive defaults and make decisions from.</param>
        /// <param name="outputDirectory">The directory to store the output audio stream after encoding/copying from the source.</param>
        /// <param name="outputBaseFilename">A base filename to use for the output file. The language and index will be appended to it.</param>
        /// <param name="additionalFlags">And additional flags to pass directly to ffmpeg.</param>
        public FFmpegAudioCommandBuilder(MediaStream audioStream, string outputDirectory, string outputBaseFilename, ICollection<string> additionalFlags)
        {
            this.audioStream = audioStream;
            this.outputDirectory = outputDirectory;
            this.outputBaseFilename = outputBaseFilename;
            commands = new List<string>
            {
                $"-map 0:{audioStream.index}"
            };

            codecSupported = Constants.SupportedOutputCodecs.ContainsKey(audioStream.codec_name);

            if (additionalFlags != null && additionalFlags.Any())
            {
                commands.AddRange(additionalFlags);
            }
        }

        /// <summary>
        /// Builds an audio stream object from all the parameters and arguments specified so far.
        /// The output path is added to the end for ffmpeg. This is an idempotent operation.
        /// </summary>
        public AudioStreamCommand Build()
        {
            path = Path.Combine(outputDirectory, $"{outputBaseFilename}_audio_{language}_{audioStream.index}.mp4");

            return new AudioStreamCommand
            {
                Index = audioStream.index,
                Name = $"{language} {title}",
                Path = path,
                Argument = string.Join(" ", commands.Union(new List<string>() { $"\"{ path}\"" }))
            };
        }

        /// <summary>
        /// Applies the given codec if the input stream is not contained in <see cref="Constants.SupportedOutputCodecs">SupportedOutputCodecs</see>.
        /// </summary>
        /// <param name="codec">The codec to use if the input is not supported. AAC is decent.</param>
        /// <param name="maxBitrate">The input bitrate is matched unless it exceeds this value, in which case it's capped here. Bits per second.</param>
        public FFmpegAudioCommandBuilder WithCodec(string codec = "aac", int maxBitrate = 1024 * 192)
        {
            if (codecSupported)
            {
                return this;
            }
            commands.Add($"-c:a {codec} -b:a {System.Math.Min(maxBitrate, audioStream.bit_rate * 1.1)}");
            return this;
        }

        /// <summary>
        /// Downmixes to two channels if necessary.
        /// </summary>
        public FFmpegAudioCommandBuilder WithDownmix(DownmixMode mode)
        {
            if (audioStream.channels > 2)
            {
                switch (mode)
                {
                    case DownmixMode.Default:
                        commands.Add("-ac 2");
                        break;

                    case DownmixMode.Nightmode:
                        commands.Add("-af \"pan=stereo|FL=FC+0.30*FL+0.30*BL|FR=FC+0.30*FR+0.30*BR\"");
                        break;

                    default:
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the given language, or a language from the input stream properties.
        /// </summary>
        /// <param name="language">An override language to use. If left null then the language is derived from the input audio stream, and set as "und" as a last resort.</param>
        public FFmpegAudioCommandBuilder WithLanguage(string language = null)
        {
            if (language == null)
            {
                language = audioStream.tag
                    .Where(x => x.key == "language")
                    .Select(x => x.value)
                    .FirstOrDefault();
            }
            if (language is null)
            {
                language = audioStream.disposition.@default > 0 ? "default" : "und";
            }
            this.language = language;
            return this;
        }

        /// <summary>
        /// Applies the given title, or a title from the input stream properties.
        /// </summary>
        /// <param name="title">An override title to use. If left null then the title is derived from the input audio stream, and set as the index as a last resort.</param>
        public FFmpegAudioCommandBuilder WithTitle(string title = null)
        {
            if (title == null)
            {
                title = audioStream.tag
                    .Where(x => x.key == "title")
                    .Select(x => x.value)
                    .FirstOrDefault();
            }
            if (title is null)
            {
                title = audioStream.index.ToString();
            }
            this.title = title;
            return this;
        }
    }
}