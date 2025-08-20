using System.IO;
using Newtonsoft.Json.Linq;

namespace Server.Helpers
{
	public static class JsonHelper
	{
		public static void WriteOk(StreamWriter w, JObject data = null)
		{
			var obj = new JObject { ["ok"] = true, ["data"] = data ?? new JObject() };
			w.WriteLine(obj.ToString(Newtonsoft.Json.Formatting.None));
		}

		public static void WriteErr(StreamWriter w, string code, string msg)
		{
			var obj = new JObject { ["ok"] = false, ["err"] = msg, ["code"] = code };
			w.WriteLine(obj.ToString(Newtonsoft.Json.Formatting.None));
		}
	}
}
