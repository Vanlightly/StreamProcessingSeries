using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InProcStreamProcessing.Shared.SensorConfiguration
{
    public class ConfigurationLoader : IConfigurationLoader
    {
        private List<SensorConfig> _sensorConfigs;

        public ConfigurationLoader()
        {
            _sensorConfigs = new List<SensorConfig>();
            _sensorConfigs.Add(new SensorConfig() { ComponentCode = "001", SensorCode = "TEMP", Unit = "DEG", ByteCount = 2, ByteIndex = 0, Precision = 0.01, Signed = true, LowerRange = -200, UpperRange = 400 });
            _sensorConfigs.Add(new SensorConfig() { ComponentCode = "001", SensorCode = "PRESSURE", Unit = "Pa", ByteCount = 2, ByteIndex = 2, Precision = 15, Signed = false, LowerRange = 0, UpperRange = 1013250 });
            _sensorConfigs.Add(new SensorConfig() { ComponentCode = "002", SensorCode = "VIB", Unit = "Hz", ByteCount = 1, ByteIndex = 4, Precision = 1.0, Signed = false, LowerRange = 0, UpperRange = 200 });
        }

        public IEnumerable<SensorConfig> GetConfigs()
        {
            return _sensorConfigs;
        }

        public IEnumerable<SensorConfig> GetConfigs(string componentCode)
        {
            return _sensorConfigs.Where(x => x.ComponentCode.Equals(componentCode));
        }

        public SensorConfig GetConfig(string componentCode, string sensorCode)
        {
            return _sensorConfigs.Single(x => x.ComponentCode.Equals(componentCode) && x.SensorCode.Equals(sensorCode));
        }
    }
}
