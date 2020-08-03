using DEnc.Models.Interfaces;
using System;
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
        public DashConfig(string inputFilePath, string outputDirectory, IEnumerable<IQuality> qualities, string outputFileName = null)
        {
            if (inputFilePath == null || !File.Exists(inputFilePath))
            {
                throw new FileNotFoundException("Input path does not exist.");
            }

            if (!Directory.Exists(outputDirectory))
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
                OutputFileName = Utilities.CleanFileName(outputFileName);
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
        /// Framerate of the video, defaults to match the input framerate
        /// </summary>
        public int Framerate { get; set; } = 0;

        /// <summary>
        /// The Full or Relative path to the Input File
        /// </summary>
        public string InputFilePath { get; }

        /// <summary>
        /// KeyframeInterval of the video, defaults to match the input keyframe interval
        /// </summary>
        public int KeyframeInterval { get; set; } = 0;

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
        public string OutputFileName { get; }

        /// <summary>
        /// A collection of <see cref="IQuality"/> items for this video
        /// </summary>
        public IEnumerable<IQuality> Qualities { get; internal set; }
    }
}