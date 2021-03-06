﻿using DEnc.Models;
using System.Collections.Generic;
using System.Linq;

namespace DEnc.Commands
{
    /// <summary>
    /// Builder for the MP4BoxCommand.
    /// Used to create the <see cref="Mp4BoxCommand"/> that's used to generate the MPD manifest
    /// </summary>
    public static class Mp4BoxCommandBuilder
    {
        /// <summary>
        /// Builds the arguments for the CLI version of MP4Box.
        /// </summary>
        /// <param name="videoFiles">A collection of full paths and roles to input video files to encode from.</param>
        /// <param name="audioFiles">A collection of full paths and roles to input audio files to encode from.</param>
        /// <param name="mpdOutputPath">The full path to write the mpd file to.</param>
        /// <param name="keyInterval">The key interval in milliseconds. This can be derived from: (keyframeInterval / framerate) * 1000</param>
        /// <param name="additionalFlags">Additional flags to pass to MP4Box. Should include a profile</param>
        internal static Mp4BoxCommand BuildMp4boxMpdCommand(
            IEnumerable<VideoStreamCommand> videoFiles,
            IEnumerable<AudioStreamCommand> audioFiles,
            string mpdOutputPath,
            int keyInterval,
            ICollection<string> additionalFlags)
        {
            ICollection<string> flags = additionalFlags ?? new List<string>();

            flags.Add($"-dash {keyInterval}");
            flags.Add($"-out \"{mpdOutputPath}\"");

            List<string> inputs = new List<string>();

            inputs.AddRange(videoFiles.Select(x => GetInputCommand(x.Bitrate, x.Path)));
            inputs.AddRange(audioFiles.Select(x => GetInputCommand(x.Name, x.Path)));

            string flagsParams = string.Join("\t", flags);
            string inputsParams = string.Join("\t", inputs.Select(x => $"\"{x}\""));

            string renderedParams = $"{flagsParams}\t--\t{inputsParams}";

            return new Mp4BoxCommand(renderedParams, mpdOutputPath);
        }

        /// <summary>
        /// Gets the Mp4Box input command for the provided role and path
        /// </summary>
        private static string GetInputCommand(string role, string path)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                return $"{path}:role={role}";
            }
            return path;
        }
    }
}