using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KelvinConsensus
{
    public class TemperatureUnit : ITemperatureUnit
    {
        private readonly int QUORUM_SIZE = 2;
        private readonly double PRECISION = 5;
        private readonly TimeSpan AUTO_SYNC_DELAY = TimeSpan.FromMinutes(1); // TODO: Implement auto sync

        private ImmutableList<ITemperatureSensor> _sensors;

        private ConcurrentQueue<double> _history = new();
        private ReaderWriterLockSlim _opLock = new();


        public TemperatureUnit() {
            _sensors = ImmutableList<ITemperatureSensor>.Empty; // TODO: Use ServiceHost to talk to sensor serivces
        }


        public double ReadTemperature() {
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
                        Console.WriteLine($"Reading from sensor {_sensors.IndexOf(sensor)} failed - {ex.Message}");
                    }
                });
            }
            finally
            {
                _opLock.ExitReadLock();
            }

            var counts = readings
                    .GroupBy(r => Math.Round(r, 2))
                    .Select(g => new { Value = g.Key, Count = g.Count() })
                    .OrderByDescending(r => r.Count)
                    .ToList();

            if (counts.Count == 0)
            {
                Console.WriteLine("Reading failed - no sensors replied");
                return double.NaN;
            }

            var consensus = counts.First();
            var dFromAverage = Math.Abs(consensus.Value - readings.Average());

            if (consensus.Count < QUORUM_SIZE || dFromAverage > PRECISION)
            {
                if (consensus.Count < QUORUM_SIZE)
                {
                    Console.WriteLine($"Reading failed - quorum size not reached ({consensus.Count}/{QUORUM_SIZE})");
                }
                else if (dFromAverage > PRECISION)
                {
                    Console.WriteLine($"Reading failed - consensus on {consensus.Value} but average is {readings.Average()}");
                }
                //Sync();
                Task.Run(Sync);
                return double.NaN;
            }

            _history.Enqueue(consensus.Value);
            return consensus.Value;
        }

        private void Sync()
        {
            _opLock.EnterWriteLock();
            try
            {
                double avgTemperature = _history.ToArray().DefaultIfEmpty(293.15).Average();
                Console.WriteLine($"Preforming sync - writing {avgTemperature}K (average temperature across past readings) to all sensors");
                Parallel.ForEach(_sensors, (sensor) => sensor.SyncTemperature(avgTemperature));
            } catch (AggregateException ex)
            {
                Console.WriteLine($"Sync failed for one or more sensors");
            }
            finally
            {
                _opLock.ExitWriteLock();
            }
        }
    }