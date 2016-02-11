using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Helper
{
	/// <summary>
	/// Tries to figure out the mime type of the file.
	/// </summary>
	/// <param name="fileName">name of the file</param>
	/// <returns>MIME type</returns>
	public static string GetMimeType(string fileName)
	{
		string mimeType = "application/unknown";
		string ext = System.IO.Path.GetExtension(fileName).ToLower();
		Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
		if(regKey != null && regKey.GetValue("Content Type") != null)
			mimeType = regKey.GetValue("Content Type").ToString();
		return mimeType;
	}
}
