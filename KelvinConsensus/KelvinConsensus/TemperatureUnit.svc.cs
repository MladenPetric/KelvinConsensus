using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KelvinConsensus
{
    public class TemperatureUnit : ITemperatureUnit
    {
        private readonly string TAG = "UNIT";
        private readonly int QUORUM_SIZE = 2, HISTORY_LIMIT = 1_000, N_SENSORS = 3;
        private readonly double PRECISION = 5;
        private readonly TimeSpan AUTO_SYNC_DELAY = TimeSpan.FromMinutes(1); // TODO: Implement auto sync

        private ImmutableList<ITemperatureSensor> _sensors;

        private ConcurrentQueue<double> _history = new ConcurrentQueue<double>();
        private ReaderWriterLockSlim _opLock = new ReaderWriterLockSlim();


        public TemperatureUnit()
        {
            _sensors = DiscoverSensors().ToImmutableList();
        }


        public double ReadTemperature()
        {
            var readings = new ConcurrentBag<double>();

            _opLock.EnterReadLock();
            try
            {
                Parallel.ForEach(_sensors, (sensor) =>
                {
                    try
                    {
                        double temp = sensor.ReadTemperature();
                        readings.Add(temp);
                    }
                    catch (FaultException ex)
                    {
                        Console.WriteLine($"[{TAG}] Reading from sensor {_sensors.IndexOf(sensor)} failed - {ex.Message}");
                    }
                });
            }
            finally
            {
                _opLock.ExitReadLock();
            }

            var counts = readings
                    .GroupBy(r => Math.Round(r, 1))
                    .Select(g => new { Value = g.Key, Count = g.Count() })
                    .OrderByDescending(r => r.Count)
                    .ToList();

            Console.WriteLine(
                $"[{TAG}] Sensor readings:" + Environment.NewLine + string.Join(Environment.NewLine, counts.Select(item => $"  Value: {item.Value:F2}, Count: {item.Count}"))
            );

            if (counts.Count == 0)
            {
                Console.WriteLine($"[{TAG}] Reading failed - no sensors replied");
                return double.NaN;
            }

            var consensus = counts.First();
            var dFromAverage = Math.Abs(consensus.Value - readings.Average());

            if (consensus.Count < QUORUM_SIZE || dFromAverage > PRECISION)
            {
                if (consensus.Count < QUORUM_SIZE)
                {
                    Console.WriteLine($"[{TAG}] Reading failed - quorum size not reached ({consensus.Count}/{QUORUM_SIZE})");
                }
                else if (dFromAverage > PRECISION)
                {
                    Console.WriteLine($"[{TAG}] Reading failed - consensus on {consensus.Value} but average is {readings.Average()}");
                }

                bool started = false;
                Task.Run(() =>
                {
                    started = true;
                    Sync();
                });

                while (!started) { }
                return double.NaN;
            }

            _history.Enqueue(consensus.Value);

            while (_history.Count > HISTORY_LIMIT)
                _history.TryDequeue(out _);

            return consensus.Value;
        }

        private void Sync()
        {
            _opLock.EnterWriteLock();
            try
            {
                double avgTemperature = _history.ToArray().DefaultIfEmpty(293.15).Average();
                Console.WriteLine($"[{TAG}] Preforming sync - writing {avgTemperature}K (average temperature across past readings) to all sensors");
                Parallel.ForEach(_sensors, (sensor) => sensor.SyncTemperature(avgTemperature));
            }
            catch (Exception)
            {
                Console.WriteLine($"[{TAG}] Sync failed for one or more sensors");
            }
            finally
            {
                _opLock.ExitWriteLock();
            }
        }

        private List<ITemperatureSensor> DiscoverSensors()
        {
            return Enumerable.Range(0, N_SENSORS)
                .Select(i => $"http://localhost:{8000 + i}/TemperatureSensor.svc")
                .Select(url => new ChannelFactory<ITemperatureSensor>(new BasicHttpBinding(), new EndpointAddress(url)).CreateChannel())
                .ToList();
        }
    }
}
