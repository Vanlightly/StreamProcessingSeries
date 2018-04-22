using System;
using System.Collections.Generic;
using System.Text;

namespace InProcStreamProcessing.Shared.SensorConfiguration
{
    public interface IConfigurationLoader
    {
        IEnumerable<SensorConfig> GetConfigs();
        IEnumerable<SensorConfig> GetConfigs(string componentCode);
        SensorConfig GetConfig(string componentCode, string sensorCode);
    }
}
