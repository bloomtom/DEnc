namespace DEnc
{
    /// <summary>
    /// Represents the progress in a single encoding stage.
    /// </summary>
    public class EncodeStageProgress
    {
        ///<inheritdoc cref="EncodeStageProgress"/>
        public EncodeStageProgress(string name, double progress)
        {
            Name = name;
            Progress = progress;
        }

        /// <summary>
        /// The name of this encode stage.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The progress in this stage as a value 0-1.
        /// </summary>
        public double Progress { get; private set; }
    }
}