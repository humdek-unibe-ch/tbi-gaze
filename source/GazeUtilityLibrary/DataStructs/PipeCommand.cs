using Newtonsoft.Json;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The JSON structure of a pipe command.
    /// </summary>
    public class PipeCommand
    {
        /// <summary>
        /// The pipe command to be sent.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Command { get; set; }

        /// <summary>
        /// An optional value associated to the command
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string? Value { get; set; }

        /// <summary>
        /// An optional flag to indicate whether the relative timestamp should be reset.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool? ResetStartTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeCommand"/> class.
        /// </summary>
        /// <param name="command">The pipe command to be sent.</param>
        /// <param name="reset">A flag to indicate whether the relative timestamp should be reset.</param>
        /// <param name="value">An optional value associated to the command</param>
        public PipeCommand(string command, bool reset, string? value) {
            Command = command;
            Value = value;
            ResetStartTime = reset;
        }
    }
}
