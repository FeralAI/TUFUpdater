using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TUFUpdater
{
	public class DeviceID
	{
		public string VID { get; set; }
		public string PID { get; set; }

		public static DeviceID FromKeyValue(KeyValuePair<string, string> keyValue)
		{
			var parts = keyValue.Value.Split(':');
			return new DeviceID { VID = parts[0].ToUpper(), PID = parts[1].ToUpper() };
		}
	}

	public class Boards
	{
		private static List<DeviceID> boards;
		public static List<DeviceID> SupportedBoards()
		{
			if (boards == null)
			{
				var json = File.ReadAllText(@$"{Directory.GetCurrentDirectory()}\data\boards.json");
				var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
				boards = dict.Select(DeviceID.FromKeyValue).ToList();
			}

			return boards;
		}
	}
}