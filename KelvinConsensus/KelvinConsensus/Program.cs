using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
                    var baseUri = new Uri(string.Format(sensorUrlFmt, 8_000 + i));
                    var host = new ServiceHost(new TemperatureSensorService(sensorNames[i]), baseUri);
                    host.AddServiceEndpoint(typeof(ITemperatureSensor), new BasicHttpBinding(), "");
                    host.Open();
                    hosts.Add(host);
                    Console.WriteLine($"Sensor {i} running at `{baseUri}`.");
                }

                var unitBaseUri = new Uri($"http://localhost:{8_000 + N_SENSORS + 1}/TemperatureUnit.svc");
                var unitHost = new ServiceHost(typeof(TemperatureUnit), unitBaseUri);
                unitHost.AddServiceEndpoint(typeof(ITemperatureUnit), new BasicHttpBinding(), "");
                unitHost.Open();
                hosts.Add(unitHost);
                Console.WriteLine($"Unit running at `{unitBaseUri}`.");

                var unit = new ChannelFactory<ITemperatureUnit>(new BasicHttpBinding(), new EndpointAddress(unitBaseUri)).CreateChannel();

                Console.WriteLine("Press enter to exit..." + Environment.NewLine);
                var exit = Task.Run(Console.ReadLine);

                Thread.Sleep(500);

                int j = 0;
                while (++j < 10 && !exit.IsCompleted)
                {
                    Console.WriteLine($"\u001b[35m======== {j:D3} - {DateTime.Now:HH:mm:ss} ========\u001b[0m");
                    double reading = unit.ReadTemperature();

                    if (!double.IsNaN(reading))
                    {
                        Console.WriteLine($"Temperature: {reading}");
                    }

                    Console.WriteLine("\u001b[35m================================\u001b[0m");
                    Thread.Sleep(2_000);
                }
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
