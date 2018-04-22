using InProcStreamProcessing.Shared.SensorConfiguration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedStreamProcessing.ApacheKafka.Decoder
{
    public class Decoder : IDecoder
    {
        private IConfigurationLoader _configurationLoader;
        private IEnumerable<SensorConfig> _sensorConfigs;

        public Decoder(IConfigurationLoader configurationLoader)
        {
            _configurationLoader = configurationLoader;
        }

        public void LoadSensorConfigs()
        {
            _sensorConfigs = _configurationLoader.GetConfigs();
        }

        public IEnumerable<DecodedMessage> Decode(RawBusMessage reading)
        {
            int decodeCounter = 0;
            foreach (var sensorConfig in _sensorConfigs)
            {
                yield return Decode(reading, sensorConfig, decodeCounter);
                decodeCounter++;
            }
        }

        public DecodedMessage Decode(RawBusMessage reading, SensorConfig sensorConfig, int decodeCounter)
        {
            var baseValue = GetDecodedBaseValue(reading.Data, sensorConfig);
            var finalValue = baseValue * sensorConfig.Precision;

            Console.WriteLine($"    Decoded Message: {reading.Counter} Sensor: {sensorConfig.SensorCode} {finalValue} {sensorConfig.Unit}");

            return new DecodedMessage()
            {
                Label = sensorConfig.SensorCode,
                ReadingTime = reading.ReadingTime,
                Source = sensorConfig.ComponentCode,
                MachineId = reading.MachineId,
                Unit = sensorConfig.Unit,
                Value = finalValue,
                Counter = (double)reading.Counter + ((double)decodeCounter / 100.0)
            };
        }

        private int GetDecodedBaseValue(byte[] data, SensorConfig sensorConfig)
        {
            if (sensorConfig.ByteCount == 1)
                return data[sensorConfig.ByteIndex];
            else if (sensorConfig.ByteCount == 2)
                return BitConverter.ToInt16(data, sensorConfig.ByteIndex);
            else if (sensorConfig.ByteCount == 4)
                return BitConverter.ToInt32(data, sensorConfig.ByteIndex);

            return 0;
        }
    }
}
