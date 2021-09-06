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
			var configBuilderForMain = new ConfigurationBuilder();
			ConfigureConfiguration(configBuilderForMain);
			var config = configBuilderForMain.Build();

			var options = config.GetSection(nameof(BoardOptions));
			var vid = options.GetValue<string>("VID");
			var pid = options.GetValue<string>("PID");
			var file = options.GetValue<string>("Filename");
			var timeout = 30000; // 30 second timeout
			
			Console.WriteLine("Please connect the controller and double press the reset button quickly to enter firmware update mode");

			var com = default(ComPort);
			var stopwatch = Stopwatch.StartNew();
			while (com == null)
			{
				if (stopwatch.ElapsedMilliseconds > timeout)
					break;
				
				var ports = GetSerialPorts();
				com = ports.FirstOrDefault(c => c.VID.Equals(vid) && c.PID.Equals(pid));
			}

			stopwatch.Stop();
			if (com != null)
			{
				Console.WriteLine("Controller found, begin firmware update");

				var port    = com.Name;
				var path    = Directory.GetCurrentDirectory();
				var avrDude = @$"{path}\avrdude.exe";
				var avrCmd  = @$"{avrDude} -v -C""{path}\avrdude.conf"" -patmega32u4 -cavr109 -P {port} -b57600 -D ""-Uflash:w:{file}:i""";
				var cmdCmd  = $"/K {avrCmd}";
				var process = Process.Start("cmd.exe", cmdCmd);

				process.WaitForExit();
			}
			else
			{
				Console.WriteLine("Controller not found, please make sure it's connected and in firmware update mode");
			}

		}

		public class ComPort
		{
			public ComPort(ManagementBaseObject p)
			{
				Name = p.GetPropertyValue("DeviceID").ToString();
				VID = p.GetPropertyValue("PNPDeviceID").ToString();
				Description = p.GetPropertyValue("Caption").ToString();

				var mVID = Regex.Match(VID, vidPattern, RegexOptions.IgnoreCase);
				var mPID = Regex.Match(PID, pidPattern, RegexOptions.IgnoreCase);

				if (mVID.Success)
					VID = mVID.Groups[1].Value;
				if (mPID.Success)
					PID = mPID.Groups[1].Value;
			}

			public string Name;
			public string VID;
			public string PID;
			public string Description;
		}

		private const string vidPattern = @"VID_([0-9A-F]{4})";
		private const string pidPattern = @"PID_([0-9A-F]{4})";

		private static List<ComPort> GetSerialPorts()
		{
			using var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort");
			return searcher.Get()
				.Cast<ManagementBaseObject>()
				.Select(p => new ComPort(p))
				.ToList();
		}
	}
}
