# Theme &numero;2

## Theme name
<strong>Consistent system</strong>  

## Project specification

* Develop an application in _WCF_ that runs 3 temperature sensors, each measures the room temperature at random itervals between 1 and 10 seconds and writes the value to a database (each sensor has its own separate database).  

* Develop a client application that will use _WCF_ to communicate with the sensors. The client app must read the values from at least 2 sensors that are within &plusmn;5 of the average of all measurements in order to consider the reading correct; otherwise, it will trigger a **synchronisation**.  

* Every minute, independently of the client app, a sensor **synchronisation** is performed via _WCF_, so that the latest value in all tables **after synchronisation** is the same across all sensors, and equal to the _average of the latest measurements_. While the sensors are being synchronised, any read requests from the client app must wait.  

* Check out _quorum based replication_ and how it may help in the development of the project.  

* See _CAP_ theorem and describe how it is applied to the project requirements.  

* Write a detailed project documentation, including a description of the project and its implementation.  