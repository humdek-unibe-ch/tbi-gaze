/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using Newtonsoft.Json;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The JSON structure of a pipe command.
    /// </summary>
    public class PipeCommand
    {
        /// <summary>
        /// The optional pipe command to be sent.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string? Command { get; set; }

        /// <summary>
        /// An optional label to annotate gaze data.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string? Label { get; set; }

        /// <summary>
        /// An optional trial ID to annotate gaze data.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int? TrialId { get; set; }

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
        /// <param name="trialId">An optional trial ID to annotate gaze data.</param>
        /// <param name="label">An optional label to annotate gaze data.</param>
        public PipeCommand(string? command, bool reset, int? trialId, string? label) {
            Command = command;
            ResetStartTime = reset;
            Label = label;
            TrialId = trialId;
        }
    }
}
