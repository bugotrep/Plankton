using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class for reading config files
/// </summary>
public static class Config
{
	/// <summary>
	/// Get AppSetting value by name
	/// </summary>
	/// <param name="key">Name of the key</param>
	/// <returns>value or empty string</returns>
	public static string Get(string key)
	{
		return ConfigurationManager.AppSettings[key] ?? string.Empty;
	}

	/// <summary>
	/// Get Connection String by name
	/// </summary>
	/// <param name="key">Name of the connection string</param>
	/// <returns>Connection string or empty string</returns>
	public static string GetConnectionString(string key)
	{
		var connection = ConfigurationManager.ConnectionStrings[key];
		return connection == null ? string.Empty : connection.ConnectionString;
	}
}