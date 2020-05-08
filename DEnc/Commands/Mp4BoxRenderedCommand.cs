using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands
{
    /// <summary>
    /// Container used to house the rendered Mp4Box command and it's output path
    /// </summary>
    internal class Mp4BoxRenderedCommand
    {
        public Mp4BoxRenderedCommand(string renderedCommand, string mpdPath)
        {
            RenderedCommand = renderedCommand;
            MpdPath = mpdPath;
        }

        public string RenderedCommand { get; private set; }
        public string MpdPath { get; private set; }
    }
}
