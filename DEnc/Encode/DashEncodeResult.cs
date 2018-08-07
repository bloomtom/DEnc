using DEnc.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DEnc
{
    public class DashEncodeResult
    {
        public MPD DashFileContent { get; protected set; }
        public TimeSpan FileDuration { get; protected set; }
        public string DashFilePath { get; protected set; }

        /// <summary>
        /// Returns the list of media filenames from the DashFileContent. This operation scans the MPD object and isn't cached.
        /// </summary>
        public IEnumerable<string> MediaFiles => DashFileContent?.Period.SelectMany(x => x.AdaptationSet.SelectMany(y => y.Representation.SelectMany(z => z.BaseURL)));

        public DashEncodeResult(MPD file, TimeSpan duration, string path)
        {
            DashFileContent = file;
            FileDuration = duration;
            DashFilePath = path;
        }
    }
}
