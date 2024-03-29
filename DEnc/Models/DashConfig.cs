﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DEnc.Models
{
    /// <summary>
    /// The dash configuration values for the <see cref="Encoder"/>
    /// </summary>
    public class DashConfig
    {
        ///<inheritdoc cref="DashConfig"/>
        /// <exception cref="FileNotFoundException">The input path does not exist.</exception>
        /// <exception cref="DirectoryNotFoundException">The output directory does not exist.</exception>
        /// <exception cref="ArgumentNullException">The qualities parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The qualities parameter is an empty set.</exception>
        /// <exception cref="ArgumentException">The set of qualities contains two or more qualities with the same bitrate.</exception>
        public DashConfig(string inputFilePath, string outputDirectory, IEnumerable<IQuality> qualities, string outputFileName = null)
        {
            if (inputFilePath == null || !File.Exists(inputFilePath))
            {
                throw new FileNotFoundException("Input path does not exist.");
            }

            if (outputDirectory == null || !Directory.Exists(outputDirectory))
            {
                throw new DirectoryNotFoundException("Output directory does not exist.");
            }

            if (qualities == null)
            {
                throw new ArgumentNullException(nameof(qualities));
            }

            if (!qualities.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(qualities), "No qualities specified. At least one quality is required.");
            }

            if (qualities.GroupBy(x => x.Bitrate).Count() != qualities.Count())
            {
                throw new ArgumentException("Duplicate quality bitrates found. Bitrates must be distinct.", nameof(qualities));
            }

            Qualities = qualities;
            InputFilePath = Path.GetFullPath(inputFilePath);  // Map input file to a full path if it's relative.
            OutputDirectory = outputDirectory;

            if (outputFileName != null)
            {
                OutputFileName = CleanFileName(outputFileName);
                if (OutputFileName.Length == 0)
                {
                    throw new ArgumentNullException("Output filename is null or empty after removal of illegal characters.");
                }
            }
            else
            {
                string name = Path.GetFileName(inputFilePath);
                string extension = Path.GetExtension(inputFilePath);
                OutputFileName = name.Replace(extension, String.Empty);
            }
        }

        /// <summary>
        /// Defines configurations to use when generating transcoding commands for audio streams.
        /// </summary>
        public AudioConfig AudioConfig { get; set; } = new AudioConfig();

        /// <summary>
        /// Specifies the minimum tolerance to use when determining quality crushing.<br/>
        /// This value is multiplied by the input file bitrate, and any bitrates less than the result are removed and replaced with a single copy quality.<br/>
        /// The default value 0.9 allows qualities up to 90% of the input bitrate before a copy quality is used instead.
        /// <br/>Set to zero to disable quality crushing. Going over 1.0 is allowed, but doesn't make a lot of sense.
        /// </summary>
        public double QualityCrushTolerance { get; set; } = 0.9;

        /// <summary>
        /// If true, the input video stream will be copied instead of re-encoded if possible.
        /// </summary>
        public bool EnableStreamCopying { get; set; } = true;

        /// <summary>
        /// Framerate of the video, defaults to match the input framerate
        /// </summary>
        public decimal Framerate { get; set; } = 0;

        /// <summary>
        /// The Full or Relative path to the Input File
        /// </summary>
        public string InputFilePath { get; }

        /// <summary>
        /// KeyframeInterval of the video, defaults to match the input keyframe interval
        /// </summary>
        public decimal KeyframeInterval { get; set; } = 0;

        /// <summary>
        /// The encoding options of the video. Defaults as <see cref="H264EncodeOptions"/>
        /// </summary>
        public IEncodeOptions Options { get; set; } = new H264EncodeOptions();

        /// <summary>
        /// The Full or Relative path of the output directory
        /// </summary>
        public string OutputDirectory { get; }

        /// <summary>
        /// The base output filename, without extension. This name is used as the base for the output names.
        /// </summary>
        public string OutputFileName { get; set; }

        /// <summary>
        /// A collection of <see cref="IQuality"/> items for this video
        /// </summary>
        public IEnumerable<IQuality> Qualities { get; internal set; }

        private static string CleanFileName(string name)
        {
            var sb = new System.Text.StringBuilder(name);
            foreach (string s in Constants.IllegalFilesystemChars)
            {
                sb.Replace(s, string.Empty);
            }
            return sb.ToString();
        }
    }
}