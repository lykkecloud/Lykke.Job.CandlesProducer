﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class ResourceMonitorSettings
    {
        public ResourceMonitorMode MonitorMode { get; set; }
        [Optional]
        public double CpuThreshold { get; set; }
        [Optional]
        public int RamThreshold { get; set; }
    }

    public enum ResourceMonitorMode
    {
        Off,
        AppInsightsOnly,
        AppInsightsWithLog
    }
}
