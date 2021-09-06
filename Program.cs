using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace TUFUpdater
{
	class Program
	{
		public static void ConfigureConfiguration(IConfigurationBuilder config)
		{
			config.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
		}

		static void Main(string[] args)
		{
			IConfigurationBuilder configBuilderForMain = new ConfigurationBuilder();
			ConfigureConfiguration(configBuilderForMain);
			IConfiguration config = configBuilderForMain.Build();

			var options = config.GetSection(nameof(BoardOptions));
			var vid = options.GetValue<string>("VID");
			var pid = options.GetValue<string>("PID");
			var file = options.GetValue<string>("Filename");
			var found = false;
			var timeout = 30000; // 30 second timeout
			
			Console.WriteLine("Please connect the controller and double press the reset button quickly to enter firmware update mode");

			var coms = new List<ComPort>();
			var stopwatch = Stopwatch.StartNew();
			while (!found)
			{
				if (stopwatch.ElapsedMilliseconds > timeout)
					break;
				
				List<ComPort> ports = GetSerialPorts();

				//if we want to find one device
				ComPort com = ports.FindLast(c => c.VID.Equals(vid) && c.PID.Equals(pid));

				//or if we want to extract all devices with specified values:
				coms = ports.FindAll(c => c.VID.Equals(vid) && c.PID.Equals(pid));
				
				found = coms.Count == 1;
			}

			stopwatch.Stop();
			if (found)
			{
				Console.WriteLine("Controller found, begin firmware update");
				var port = coms[0].Name;
				var path = Directory.GetCurrentDirectory();
				var avrDude = @$"{path}\avrdude.exe";
				var avrCmd = @$"{avrDude} -v -C""{path}\avrdude.conf"" -patmega32u4 -cavr109 -P {port} -b57600 -D ""-Uflash:w:{file}:i""";
				var cmdCmd = $"/K {avrCmd}";
				var process = Process.Start("cmd.exe", cmdCmd);
				process.WaitForExit();
			}
			else
			{
				Console.WriteLine("Controller not found, please make sure it's connected and in firmware update mode");
			}

		}

		// You can define other methods, fields, classes and namespaces here
		public struct ComPort // custom struct with our desired values
		{
			public string Name;
			public string VID;
			public string PID;
			public string Description;
		}

		private const string vidPattern = @"VID_([0-9A-F]{4})";
		private const string pidPattern = @"PID_([0-9A-F]{4})";

		private static List<ComPort> GetSerialPorts()
		{
			using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
			{
				var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
				return ports.Select(p =>
				{
					ComPort c = new ComPort();
					c.Name = p.GetPropertyValue("DeviceID").ToString();
					c.VID = p.GetPropertyValue("PNPDeviceID").ToString();
					c.Description = p.GetPropertyValue("Caption").ToString();

					Match mVID = Regex.Match(c.VID, vidPattern, RegexOptions.IgnoreCase);
					Match mPID = Regex.Match(c.VID, pidPattern, RegexOptions.IgnoreCase);

					if (mVID.Success)
						c.VID = mVID.Groups[1].Value;
					if (mPID.Success)
						c.PID = mPID.Groups[1].Value;

					return c;

				}).ToList();
			}
		}
	}
}
