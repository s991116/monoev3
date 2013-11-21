using System;
using MonoBrickFirmware.IO;
using System.Threading;
using MonoBrickFirmware.Graphics;
namespace LightSensorExample
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ManualResetEvent terminateProgram = new ManualResetEvent(false);
			var lightSensor = new LightSensor(SensorPort.In1);
			ButtonEvents buts = new ButtonEvents ();
			lightSensor.Initialize();
			buts.EnterPressed += () => { 
				terminateProgram.Set();
			};
			buts.UpPressed += () => { 
				LcdConsole.WriteLine("Sensor value:" + lightSensor.ReadAsString());
			};
			buts.DownPressed += () => { 
				if(lightSensor.Mode == LightMode.Ambient){
					lightSensor.Mode = LightMode.Relection;
				}
				else{
					lightSensor.Mode = LightMode.Ambient;
				}
				LcdConsole.WriteLine("Sensor mode is now: " + lightSensor.Mode);
			};  
			terminateProgram.WaitOne();
		}
	}
}
