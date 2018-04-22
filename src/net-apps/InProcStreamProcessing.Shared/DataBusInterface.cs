using InProcStreamProcessing.Shared.SensorConfiguration;
using System;
using System.Collections.Generic;

namespace InProcStreamProcessing.Shared
{
    public class DataBusInterface
    {
        private Dictionary<string, double> _maxValues;
        private Dictionary<string, double> _lastSensorValues;
        private Random _random;
        private ConfigurationLoader _configLoader;

        public void Initialize()
        {
            _configLoader = new ConfigurationLoader();

            _maxValues = new Dictionary<string, double>();
            _maxValues.Add("TEMP", 30000);
            _maxValues.Add("PRESSURE", 50000);
            _maxValues.Add("VIB", 200);

            _lastSensorValues = new Dictionary<string, double>();
            _lastSensorValues.Add("TEMP", 0);
            _lastSensorValues.Add("PRESSURE", _maxValues["PRESSURE"] / 2);
            _lastSensorValues.Add("VIB", _maxValues["VIB"] / 2);

            _random = new Random();
        }

        public string GetMachineId()
        {
            return "MD63FO1";
        }

        public BusMessage Read()
        {
            // here we would interact with a C library to read the current value
            // but we'll just fake something

            var now = DateTime.Now;
            var reading = new BusMessage();
            reading.XYZ = Guid.NewGuid().ToString();
            reading.Ticks = now.Ticks;
            reading.Data = new byte[8];

            var tempVal = GetTwoByteEncoding(GetNextValue("001", "TEMP"));
            var pressureVal = GetTwoByteEncoding(GetNextValue("001", "PRESSURE"));
            var vib = GetSingleByteEncoding(GetNextValue("002", "VIB"));

            reading.Data[0] = tempVal[0];
            reading.Data[1] = tempVal[1];
            reading.Data[2] = pressureVal[0];
            reading.Data[3] = pressureVal[1];
            reading.Data[4] = vib;
            
            return reading;
        }

        private byte GetSingleByteEncoding(int value)
        {
            return BitConverter.GetBytes(value)[0];
        }

        private byte[] GetTwoByteEncoding(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            return new byte[] { bytes[0], bytes[1] };
        }

        private int GetNextValue(string componentCode, string sensorCode)
        {
            var sensorConfig = _configLoader.GetConfig(componentCode, sensorCode);
            var lastValue = _lastSensorValues[sensorCode];
            double modifier = (_random.NextDouble() / 100.0); // up to 1%
            if (_random.Next(10) % 2 == 1)
                modifier = 1.0 + modifier;
            else
                modifier = 1.0 - modifier;

            double newValue = lastValue * modifier;

            var maxVal = _maxValues[sensorCode];
            if (sensorConfig.Signed)
            {
                if (newValue >= maxVal)
                    newValue -= maxVal * 0.1;
                else if (newValue <= -_maxValues[sensorCode])
                    newValue += maxVal * 0.1;
            }
            else
            {
                if (newValue >= _maxValues[sensorCode])
                    newValue = maxVal * 0.9;
            }

            _lastSensorValues[sensorCode] = newValue;

            return (int)newValue;
        }
    }
}
