﻿using System;
using System.Windows;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    public class EyeTrackerCore : EyeTrackerHandler
    {
        private Host host;

        public event Action<double, double, double> GazeDataReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerCore"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        public EyeTrackerCore(TrackerLogger logger, int ready_timer) : base(logger, ready_timer)
        {
            logger.Info("Using Tobii SDK Core");
            host = new Host();
            EngineStateObserver<EyeTrackingDeviceStatus> deviceStateObserver = host.States.CreateEyeTrackingDeviceStatusObserver();
            deviceStateObserver.Changed += OnUpdateDeviceStatus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerCore"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        /// <param name="gaze_filter_mode">The gaze filter mode.</param>
        public EyeTrackerCore(TrackerLogger logger, int ready_timer, int gaze_filter_mode) : this(logger, ready_timer)
        {
            GazePointDataStream gazePointDataStream = host.Streams.CreateGazePointDataStream(GetFilterMode(gaze_filter_mode));
            gazePointDataStream.GazePoint((x, y, ts) => GazeDataReceived(x, y, ts));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                host.DisableConnection();
                host.Dispose();
            }
        }

        /// <summary>
        /// Gets the filter mode.
        /// </summary>
        /// <param name="gaze_filter_mode">The gaze filter mode.</param>
        /// <returns></returns>
        private GazePointDataMode GetFilterMode(int gaze_filter_mode)
        {
            GazePointDataMode filter;
            switch (gaze_filter_mode)
            {
                case 0: filter = GazePointDataMode.Unfiltered; break;
                case 1: filter = GazePointDataMode.LightlyFiltered; break;
                default:
                    filter = GazePointDataMode.Unfiltered;
                    logger.Error($"Unkonwn filter setting: \"{gaze_filter_mode}\"");
                    logger.Warning("Using unfiltered mode");
                    break;
            }
            return filter;
        }

        /// <summary>
        /// Called when the state of eye tracker is updated.
        /// </summary>
        /// <param name="deviceState">State of the eyetracker device.</param>
        private void OnUpdateDeviceStatus(object sender, EngineStateValue<EyeTrackingDeviceStatus> deviceState)
        {
            State = deviceState.Value;
        }
    }
}