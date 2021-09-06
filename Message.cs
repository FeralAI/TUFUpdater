using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace TUFUpdater
{
	public static class Message
	{
		private static Dictionary<string, string> messages;

		static Message()
		{
				var json = File.ReadAllText(@$"{Directory.GetCurrentDirectory()}\data\messages.json");
				messages = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
				var props = typeof(Message).GetProperties(BindingFlags.Public | BindingFlags.Static);
				foreach (var prop in props)
					prop.SetValue(null, messages.GetValueOrDefault(prop.Name));
		}

		public static string Start { get; private set; }
		public static string Connect { get; private set; }
		public static string Found { get; private set; }
		public static string NotFound { get; private set; }
	}
}