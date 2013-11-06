using System;
using MonoBrickFirmware.Native;

namespace MonoBrickFirmware.IO
{
	/// <summary>
	/// Sensor modes
	/// </summary>
	public enum I2CMode {
		#pragma warning disable 
		LowSpeed = AnalogMode.Set, LowSpeed9V = AnalogMode.Set | AnalogMode.Pin1
		#pragma warning restore
	};
	
	/// <summary>
	/// Base class for all I2C sensors. This should be used when implementing a new I2C sensor
	/// </summary>
	public abstract class I2CAbstraction
	{
		private UnixDevice I2CDevice;
		//private MemoryArea I2CMemory;
		
		private const int InitDelay = 100;
		private const int DataLength = 30;
		
		//I2C control 
		private const UInt32 I2CIOSetup = 0xc04c6905;
		
		
		protected byte I2CAddress = 0x00;
		
		protected const int NumberOfSenosrPorts = SensorManager.NumberOfSenosrPorts;
		protected SensorPort port;
		protected UARTMode uartMode{get; private set;}
		protected I2CMode mode;
		
		public I2CAbstraction (SensorPort port, byte address, I2CMode mode)
		{
			this.port = port;
			this.I2CAddress = address;
			//I2CMemory = SensorManager.Instance.I2CMemory;
			I2CDevice = SensorManager.Instance.I2CDevice;
			this.mode = mode;
			SensorManager.Instance.SetAnalogMode((AnalogMode)mode, port);
			
		}
		
		protected void Reset ()
		{
			SensorManager.Instance.ResetI2C(this.port);
		}
		
		protected void SetMode ()
		{
			SensorManager.Instance.SetI2COperatingMode(this.port);
		}
		
		protected bool Initialise()
	    {
	       	SensorManager.Instance.SetAnalogMode(AnalogMode.Pin5, this.port);
	       	Reset();
	       	System.Threading.Thread.Sleep(InitDelay);
	       	SetMode();
	       	System.Threading.Thread.Sleep(InitDelay);
	       	SetMode();
	       	System.Threading.Thread.Sleep(InitDelay);
	       	return true;
	    }
	    
	    
	    /// <summary>
		/// Reads a 8 byte register from the sensor
		/// </summary>
		/// <returns>
		/// The bytes that was read
		/// </returns>
		/// <param name='register'>
		/// Register to read
		/// </param>
		protected byte[] ReadRegister(byte register){
			return ReadRegister(register,8);
		}

		/// <summary>
		/// Reads a register from the sensor
		/// </summary>
		/// <returns>
		/// The bytes that was read
		/// </returns>
		/// <param name='register'>
		/// Register to read
		/// </param>
		/// <param name='rxLength'>
		/// The number of bytes to read
		/// </param>
  		protected byte[] ReadRegister(byte register, byte rxLength)
        {
           	//byte[] command = { I2CAddress, register };
           	byte[] command = {};
           	return WriteAndRead(register, command, rxLength);
        }

		/// <summary>
		/// Writes a byte to a register.
		/// </summary>
		/// <param name='register'>
		/// Register to write to
		/// </param>
		/// <param name='data'>
		/// Data byte to write
		/// </param>
		protected void WriteRegister(byte register, byte data) {
            //byte[] command = { I2CAddress, register, data};
            byte[] command = {data};
            WriteAndRead(register, command, 0);
        }
        
        protected void WriteRegister(byte register, byte[] data) {
            //byte[] command = { I2CAddress, register, data};
            WriteAndRead(register, data, 0);
        }
        

       	/// <summary>
		/// Write and read an array of bytes to the sensor
		/// </summary>
		/// <returns>The bytes that was read</returns>
		/// <param name="register">Register to write to.</param>
		/// <param name="data">Byte array to write</param>
		/// <param name="rxLength">Length of the expected reply</param>
        protected byte[] WriteAndRead (byte register, byte[] data, int rxLength)
		{
			bool dataReady = false;
			int replyIndex = 0;
			byte[] writeData = new byte[DataLength];//32
			Array.Copy (data, 0, writeData, 0, data.Length);
			DeviceCommand command = new DeviceCommand ();
			command.Append ((int)-1);
			command.Append ((byte)this.port);
			command.Append ((byte)1);//repeat
			command.Append ((short)0);//time
			command.Append ((byte)(data.Length + 2));//length of write data
			command.Append ((byte)((byte)I2CAddress >> 1));
			command.Append (register);
			command.Append(writeData);
			command.Append ((byte)-rxLength);
			replyIndex = command.Data.Length;
			command.Append (new byte[DataLength]);//make room for reply
			byte[] i2cData = command.Data;
			while (!dataReady) {
				unchecked {
					I2CDevice.IoCtl ((Int32)I2CIOSetup, i2cData);
				}
				int status = BitConverter.ToInt32 (i2cData, 0);
				if (status < 0) {
					throw new Exception ("I2C I/O error");
				}
				if (status == 0) {
					byte[] reply = new byte[rxLength];
					if (rxLength > 0) {
						Array.Copy(i2cData,replyIndex, reply,0, rxLength);
					}
					return reply;
				}
			}
        	throw new TimeoutException("I2C timeout");
    	}
	}
	
	/// <summary>
	/// I2C sensor class for writing and reading.
	/// </summary>
	public sealed class I2CSensor : I2CAbstraction{
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrickFirmware.IO.I2CSensor"/> class with I2C address 0x02
		/// </summary>
		/// <param name="port">Sensor port to use</param>
		/// <param name="mode">I2C mode to use</param>
		public I2CSensor (SensorPort port, I2CMode mode): this(port, 0x02, mode)
		{
			
		
		}
		
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoBrickFirmware.IO.I2CSensor"/> class.
		/// </summary>
		/// <param name="port">Sensor port to use.</param>
		/// <param name="address">I2C address to use</param>
		/// <param name="mode">I2C mode to use</param>
		public I2CSensor (SensorPort port, byte address, I2CMode mode):base(port,address, mode)
		{
			base.Initialise();
		
		}
		
		/// <summary>
		/// Reads a 8 byte register from the sensor
		/// </summary>
		/// <returns>
		/// The bytes that was read
		/// </returns>
		/// <param name='register'>
		/// Register to read
		/// </param>
		public new byte[] ReadRegister(byte register){
			return base.ReadRegister(register);
		}

		/// <summary>
		/// Reads a register from the sensor
		/// </summary>
		/// <returns>
		/// The bytes that was read
		/// </returns>
		/// <param name='register'>
		/// Register to read
		/// </param>
		/// <param name='rxLength'>
		/// The number of bytes to read
		/// </param>
  		public new byte[] ReadRegister(byte register, byte rxLength)
        {
           	return base.ReadRegister(register,rxLength);
        }

		/// <summary>
		/// Writes a byte to a register.
		/// </summary>
		/// <param name='register'>
		/// Register to write to
		/// </param>
		/// <param name='data'>
		/// Data byte to write
		/// </param>
		public new void WriteRegister(byte register, byte data) {
           base.WriteRegister(register,data);
        }
        
        /// <summary>
        /// Writes the register.
        /// </summary>
        /// <param name="register">Register to write to</param>
        /// <param name="data">Data bytes to write</param>
        public new void WriteRegister(byte register, byte[] data) {
           base.WriteRegister(register,data);
        }
	}
	
	
	
}

