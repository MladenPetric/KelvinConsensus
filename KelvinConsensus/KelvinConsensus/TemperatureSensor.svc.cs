using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace KelvinConsensus
{
    public class TemperatureSensor : ITemperatureSensor
    {
        // TODO: Implement temperature sensor service
        double ReadTemperature() => double.NaN; 
        void SyncTemperature(double value) {}
    }
}