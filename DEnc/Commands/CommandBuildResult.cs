using System;
using System.Collections.Generic;
using System.Text;

namespace DEnc.Commands
{
    internal class CommandBuildResult
    {
        public string RenderedCommand { get; private set; }
        public IEnumerable<StreamFile> CommandPieces { get; private set; }

        internal CommandBuildResult(string commandArguments, IEnumerable<StreamFile> commands)
        {
            RenderedCommand = commandArguments;
            CommandPieces = commands;
        }
    }
}
