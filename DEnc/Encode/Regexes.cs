using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DEnc.Encode
{
    public static class Regexes
    {
        public static Regex ParseProgress { get; } = new Regex(@"(?<=frame=.+time=)\d\S+", RegexOptions.Compiled);
    }
}
