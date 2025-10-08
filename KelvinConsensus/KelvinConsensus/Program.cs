using System;
using System.ServiceModel;
using System.Collections.Generic;

namespace KelvinConsensus
{

    class Program
    {
        static void Main(string[] args)
        {
            var sensorUrlFmt = "http://localhost:{0}/TemperatureSensor.svc";
            var N_SENSORS = 3;
            var sensorNames = (SensorName[])Enum.GetValues(typeof(SensorName));
            var hosts = new List<ServiceHost>();

            try
            {
                for (int i = 0; i < N_SENSORS; ++i) {
                    var baseUri = new Uri(string.Format(sensorUrlFmt, i));
                    var host = new ServiceHost(new TemperatureSensorService(sensorNames[i]), baseUri);
                    host.AddServiceEndpoint(typeof(ITemperatureSensor), new BasicHttpBinding(), "");
                    host.Open();
                    hosts.Add(host);
                    Console.WriteLine($"Sensor {i} running at `{baseUri}`.");
                }

                var unitBaseUri = new Uri($"http://localhost:{N_SENSORS + 1}/TemperatureUnit.svc");
                var unitHost = new ServiceHost(typeof(TemperatureUnit), unitBaseUri);
                unitHost.AddServiceEndpoint(typeof(ITemperatureUnit), new BasicHttpBinding(), "");
                unitHost.Open();
                hosts.Add(unitHost);
                Console.WriteLine($"Unit running at `{unitBaseUri}`.");
                
                Console.ReadLine();
            } finally
            {
                foreach (var host in hosts)
                {
                    host.Close();
                }
            }
        }
    }

}
