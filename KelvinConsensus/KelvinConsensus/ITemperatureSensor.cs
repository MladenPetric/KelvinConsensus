using System;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace KelvinConsensus
{
	[ServiceContract]
	public interface ITemperatureSensor
    {
		[OperationContract]
		[FaultContract(typeof(string))]
		double ReadTemperature();

		[OperationContract]
		void SyncTemperature(double value);
	}
}