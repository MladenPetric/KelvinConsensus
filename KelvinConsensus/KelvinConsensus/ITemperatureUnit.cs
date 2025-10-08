using System;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace KelvinConsensus
{
    [ServiceContract]
    public interface ITemperatureUnit
    {
        [OperationContract]
        double ReadTemperature();
    }
}