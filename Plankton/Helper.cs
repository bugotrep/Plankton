using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public static class Helper
{
	/// <summary>
	/// Serialize class to JSON
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="model">class</param>
	/// <returns>JSON</returns>
	public static string ToJson<T>(T model) where T : class
	{
		return JsonConvert.SerializeObject(model, new JsonSerializerSettings()
		{
			DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
		});
	}
}
