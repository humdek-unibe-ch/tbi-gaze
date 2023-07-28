using Newtonsoft.Json;

namespace GazeUtilityLibrary.DataStructs
{
    public class PipeCommand
    {
        [JsonProperty(Required = Required.Always)]
        public string Command { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string? Value { get; set; }

        [JsonProperty(Required = Required.Default)]
        public bool? ResetStartTime { get; set; }

        public PipeCommand(string command, bool reset, string? value) {
            Command = command;
            Value = value;
            ResetStartTime = reset;
        }
    }
}
