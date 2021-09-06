using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TUFUpdater
{
	class Program
	{
		static void Main(string[] args)
		{
			var file = args.FirstOrDefault();
			if (string.IsNullOrWhiteSpace(file))
			{
				Console.WriteLine(Message.Start);
				return;
			}

			var timeout = 30000; // 30 second timeout
			
			Console.WriteLine(Message.Connect);

			var comm = default(Comm);
			var stopwatch = Stopwatch.StartNew();
			while (comm == null)
			{
				if (stopwatch.ElapsedMilliseconds > timeout)
					break;
				
				var ports = Comm.GetSerialPorts();
				comm = ports.Join(
					Boards.SupportedBoards(),
					c => new { c.VID, c.PID },
					b => new { b.VID, b.PID },
					(c, b) => c
				).FirstOrDefault();
			}

			stopwatch.Stop();
			if (comm != null)
			{
				Console.WriteLine(Message.Found);

				var path    = Directory.GetCurrentDirectory();
				var avrCmd  = @$"{path}\tools\avrdude.exe -v -C""{path}\tools\avrdude.conf"" -patmega32u4 -cavr109 -P {comm.DeviceID} -b57600 -D ""-Uflash:w:""{file}"":i""";
				var process = Process.Start("cmd.exe", $"/K {avrCmd}");

				process.WaitForExit();
			}
			else
			{
				Console.WriteLine(Message.NotFound);
			}
		}
	}
}
