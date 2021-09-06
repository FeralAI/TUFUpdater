using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace TUFUpdater
{
	public class Comm
	{
		private const string vidPattern = @"VID_([0-9A-F]{4})";
		private const string pidPattern = @"PID_([0-9A-F]{4})";

		public static List<Comm> GetSerialPorts()
		{
			using var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort");

			return searcher.Get()
				.Cast<ManagementBaseObject>()
				.Select(Comm.FromMBO)
				.ToList();
		}

		public static Comm FromMBO(ManagementBaseObject p)
		{
			var comm = new Comm
			{
				DeviceID = p.GetPropertyValue("DeviceID").ToString(),
				VID = p.GetPropertyValue("PNPDeviceID").ToString(),
				Description = p.GetPropertyValue("Caption").ToString(),
			};

			var mVID = Regex.Match(comm.VID, vidPattern, RegexOptions.IgnoreCase);
			var mPID = Regex.Match(comm.VID, pidPattern, RegexOptions.IgnoreCase);

			if (mVID.Success)
				comm.VID = mVID.Groups[1].Value.ToUpper();
			if (mPID.Success)
				comm.PID = mPID.Groups[1].Value.ToUpper();

			return comm;
		}

		public string DeviceID { get; set; }
		public string VID { get; set; }
		public string PID { get; set; }
		public string Description { get; set; }
	}
}