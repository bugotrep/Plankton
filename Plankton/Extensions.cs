using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Dynamite.Extensions;
using Newtonsoft.Json;

/// <summary>
/// Class for variable part of extensions
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Check if string is null or empty
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static bool IsNullOrEmpty(this string s)
	{
		return string.IsNullOrEmpty(s);
	}

	public static decimal ToDecimal(this string s, decimal Default = 0)
	{
		decimal result = Default;
		return decimal.TryParse(s, out result) ? result : Default;
	}

	public static DateTime? ToDate(this string s, string pattern = "d.M.yyyy")
	{
		try
		{
			return DateTime.ParseExact(s.Trim(), pattern, CultureInfo.InvariantCulture);
		}
		catch
		{
			return null;
		}
	}

	public static DateTime ToDateTime(this string s)
	{
		try
		{
			return DateTime.Parse(s.Trim(), CultureInfo.CurrentCulture);
		}
		catch
		{
			return DateTime.MinValue;
		}
	}

	public static long ToLong(this string s, long Default = 0)
	{
		long result = Default;
		return long.TryParse(s, out result) ? result : Default;
	}

	public static bool ToBool(this string s, bool Default = false)
	{
		bool result = Default;
		return bool.TryParse(s, out result) ? result : Default;
	}

	public static Guid ToGuid(this string s, Guid Default = default(Guid))
	{
		Guid result = Default;
		return Guid.TryParse(s, out result) ? result : Default;
	}

	public static bool? ToBoolNullable(this string s, bool? Default = null)
	{
		bool result = Default.GetValueOrDefault();
		if(s.IsNullOrEmpty())
		{
			return null;
		}
		return bool.TryParse(s, out result) ? result : Default;
	}

	public static int? ToIntNullable(this string s, int? Default = null)
	{
		int result = Default.GetValueOrDefault();
		if(s.IsNullOrEmpty())
		{
			return null;
		}
		return int.TryParse(s, out result) ? result : Default;
	}

	public static ICollection<T> RemoveBy<T>(this ICollection<T> list, Func<T, bool> match)
	{
		foreach(T found in list
			.Where(i => match(i))
			.ToList())
		{
			list.Remove(found);
		}
		return list;
	}

	public static IOrderedQueryable<T> OrderBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> exp, bool asc)
	{
		return asc ? query.OrderBy(exp) : query.OrderByDescending(exp);
	}

	public static int ToInt(this string s, int Default = 0)
	{
		int result = Default;
		return int.TryParse(s, out result) ? result : Default;
	}

	/// <summary>
	/// Check If Type is Nullable
	/// </summary>
	/// <param name="theType"></param>
	/// <returns></returns>
	public static bool IsNullableType(this Type theType)
	{
		return (theType.IsGenericType && theType.
		  GetGenericTypeDefinition().Equals
		  (typeof(Nullable<>)));
	}

	/// <summary>
	/// Convert linq query result to DataTable object
	/// </summary>
	/// <typeparam name="T">Type of result</typeparam>
	/// <param name="varlist">query</param>
	/// <returns>datatable</returns>
	public static DataTable ToDataTable<T>(this IEnumerable<T> varlist, IDictionary<string, string> columnNames = null)
	{
		DataTable dtReturn = new DataTable();

		// Use reflection to get property names, to create table
		// column names
		PropertyInfo[] oProps = typeof(T).GetProperties();
		foreach(PropertyInfo pi in oProps)
		{
			string caption = columnNames != null && columnNames.ContainsKey(pi.Name) ? columnNames[pi.Name] : pi.Name;
			DataColumn col = new DataColumn(pi.Name, pi.PropertyType.IsNullableType() ? pi.PropertyType.BaseType : pi.PropertyType);
			col.Caption = caption;
			dtReturn.Columns.Add(col);
		}

		foreach(T rec in varlist)
		{
			DataRow dr = dtReturn.NewRow();
			foreach(PropertyInfo pi in oProps)
				dr[pi.Name] = pi.GetValue(rec, null);
			dtReturn.Rows.Add(dr);
		} // foreach

		return (dtReturn);
	}

	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		foreach(var item in source)
			action(item);
	}

	public static IQueryable<T> Sort<T>(this IQueryable<T> source, string sortExpression)
	{
		if(sortExpression.IsNullOrEmpty())
		{
			return source;
		}
		try
		{
			return source.OrderBy(sortExpression);
		}
		catch(ArgumentException)
		{
			return source;
		}
	}

	public static IQueryable<T> Sort<T, K>(this IQueryable<T> source, string sortExpression, Expression<Func<T, K>> defaultSort)
	{
		if(sortExpression.IsNullOrEmpty())
		{
			return source.OrderBy(defaultSort);
		}
		else
		{
			try
			{
				return source.OrderBy(sortExpression);
			}
			catch(ArgumentException)
			{
				return source;
			}
		}
	}

	public static IQueryable<T> Sort<T, K>(this IQueryable<T> source, string sortExpression, Expression<Func<T, K>> defaultSort, bool asc = true)
	{
		if(sortExpression.IsNullOrEmpty())
		{
			if(asc)
			{
				return source.OrderBy(defaultSort);
			}
			else
			{
				return source.OrderByDescending(defaultSort);
			}
		}
		else
		{
			try
			{
				return source.OrderBy(sortExpression);
			}
			catch(ArgumentException)
			{
				return source;
			}
		}
	}

	public static string EndWith(this string s, string suffix)
	{
		if(!s.EndsWith(suffix))
		{
			s += suffix;
		}
		return s;
	}

	public static IOrderedQueryable<TEntity> OrderUsingSortExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression, string defaultSort) where TEntity : class
	{
		if(sortExpression.IsNullOrEmpty())
			sortExpression = defaultSort;
		string[] orderFields = sortExpression.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		IOrderedQueryable<TEntity> result = null;
		for(int currentFieldIndex = 0; currentFieldIndex < orderFields.Length; currentFieldIndex++)
		{
			string[] expressionPart = orderFields[currentFieldIndex].Trim().Split(' ');
			string sortField = expressionPart[0];
			bool sortDescending = (expressionPart.Length == 2)
				&& (expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase));
			if(sortDescending)
			{
				result = currentFieldIndex == 0 ? source.OrderByDescending(sortField) : result.ThenByDescending(sortField);
			}
			else
			{
				result = currentFieldIndex == 0 ? source.OrderBy(sortField) : result.ThenBy(sortField);
			}
		}
		return result;
	}

	public static IQueryable<TEntity> OrderUsingSortExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression, string defaultSort,
		int startRow, int pageSize) where TEntity : class
	{
		return source.OrderUsingSortExpression(sortExpression, defaultSort).Page(startRow, pageSize);
	}

	public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName, bool bAscending) where TEntity : class
	{
		return bAscending ? source.OrderBy(fieldName) : source.OrderByDescending(fieldName);
	}

	//public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity: class
	//{
	//    MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderBy", fieldName);
	//    return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
	//}

	public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
	{
		MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderByDescending", fieldName);
		return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
	}

	public static IOrderedQueryable<TEntity> ThenBy<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
	{
		MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenBy", fieldName);
		return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
	}

	public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
	{
		MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenByDescending", fieldName);
		return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
	}

	private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType) where TEntity : class
	{
		// Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
		var parameter = Expression.Parameter(typeof(TEntity), "Entity");
		//  create the selector part, but support child properties
		PropertyInfo property;
		Expression propertyAccess;
		if(propertyName.Contains('.'))
		{
			// support to be sorted on child fields.
			String[] childProperties = propertyName.Split('.');
			property = typeof(TEntity).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			propertyAccess = Expression.MakeMemberAccess(parameter, property);
			for(int i = 1; i < childProperties.Length; i++)
			{
				property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
			}
		}
		else
		{
			property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			propertyAccess = Expression.MakeMemberAccess(parameter, property);
		}
		resultType = property.PropertyType;
		// Create the order by expression.
		return Expression.Lambda(propertyAccess, parameter);
	}

	private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName) where TEntity : class
	{
		Type type = typeof(TEntity);
		Type selectorResultType;
		LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
		MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName,
						new Type[] { type, selectorResultType },
						source.Expression, Expression.Quote(selector));
		return resultExp;
	}

	/// <summary>
	/// Return true if string is null or empty or consists only white-space characters
	/// </summary>
	/// <param name="s">string</param>
	/// <returns>true if string is empty or consists only white-space characters</returns>
	public static bool IsNullOrWhiteSpace(this string s)
	{
		return string.IsNullOrWhiteSpace(s);
	}

	public static IQueryable<T> Page<T>(this IQueryable<T> query, int startRow, int pageSize)
	{
		return pageSize > 0 ? query
			.Skip(startRow)
			.Take(pageSize) : query;
	}

	public static T IfNull<T>(this T item, T defaultValue)
	{
		return item == null ? defaultValue : item;
	}

	public static string IfNull(this string item, string defaultValue)
	{
		return item.IsNullOrWhiteSpace() ? defaultValue : item;
	}

	/// <summary>
	/// Perform a deep Copy of the object.
	/// </summary>
	/// <typeparam name="T">The type of object being copied.</typeparam>
	/// <param name="source">The object instance to copy.</param>
	/// <returns>The copied object.</returns>
	public static T Clone<T>(T source) where T : class
	{
		if(!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}

		// Don't serialize a null object, simply return the default for that object
		if(Object.ReferenceEquals(source, null))
		{
			return default(T);
		}

		IFormatter formatter = new BinaryFormatter();
		Stream stream = new MemoryStream();
		using(stream)
		{
			formatter.Serialize(stream, source);
			stream.Seek(0, SeekOrigin.Begin);
			return (T) formatter.Deserialize(stream);
		}
	}

	public static string ToJson<T>(this T model) where T : class
	{
		return JsonConvert.SerializeObject(model, new JsonSerializerSettings()
		{
			DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
			StringEscapeHandling = StringEscapeHandling.EscapeHtml,
		});
	}

	public static DateTime FixDatePickerTime(this DateTime dt)
	{
		return dt.ToLocalTime();
	}

	public static DateTime? FixDatePickerTime(this DateTime? dt)
	{
		if(dt.HasValue)
		{
			return dt.Value.ToLocalTime();
		}
		return dt;
	}

	public static HashSet<T> AddMany<T>(this HashSet<T> hash, IEnumerable<T> items)
	{
		foreach(T item in items)
		{
			hash.Add(item);
		}
		return hash;
	}

	#region	ToSpeech

	public static string ToSpeechBG(this decimal value)
	{
		var currencyString = "лева";
		var strRet = new StringBuilder();
		value = Math.Round(value, 2);

		var bMinus = false;
		if(value < 0)
		{
			value = Math.Abs(value);
			bMinus = true;
		}

		var str = string.Empty;
		var bPutAnd = false;
		var whole = Math.Floor(value);
		var reminder = Math.Round(value - whole, 2);

		var d = whole / 1000;
		d = Math.Round(d - Math.Floor(d), 3) * 1000;
		if(d <= 100)
			bPutAnd = true;
		if(Math.Floor(d) % 100 == 0)
			bPutAnd = true;
		strRet.Append(ToSpeech(Math.Floor(Math.Round(d, 0)), false));

		whole /= 1000;
		whole = Math.Floor(whole);
		if(whole == 0)
		{
			currencyString = "лева";
			if(Math.Floor(value) == 1)
				currencyString = "лев";

			if(reminder > 0)
			{
				reminder *= 100;
				str = reminder.ToString("0");
				if(strRet.Length > 0 && str.Length > 0)
				{
					strRet.Append(" ").Append(currencyString).Append(" и ").Append(str).Append(" ст.");
				}
				else
				{
					strRet = new StringBuilder();
					strRet.Append(str).Append(" ст.");
				}
			}
			else
			{
				strRet.Append(" ").Append(currencyString);
			}

			if(value == 0)
			{
				strRet = new StringBuilder();
				strRet.Append("0 ").Append(currencyString);
			}

			if(bMinus == true)
				strRet.Insert(0, "минус ");
			return strRet.ToString();
		}
		d = whole / 1000;
		d = Math.Round(d - Math.Floor(d), 3) * 1000;
		if(Math.Floor(d) == 1)
		{
			if(bPutAnd == true && strRet.Length > 0)
			{
				strRet.Insert(0, " и ");
				bPutAnd = false;
			}
			strRet.Insert(0, "хиляда ");
		}
		else if(Math.Floor(d) != 0)
		{
			if(bPutAnd == true && strRet.Length > 0)
			{
				strRet.Insert(0, " и ");
				bPutAnd = false;
			}
			strRet.Insert(0, " хиляди ").Insert(0, ToSpeech(Math.Floor(d), true));
		}
		whole /= 1000;
		whole = Math.Floor(whole);
		if(whole == 0)
		{
			currencyString = "лева";
			if(Math.Floor(value) == 1)
				currencyString = "лев";

			if(reminder > 0)
			{
				reminder *= 100;
				str = reminder.ToString("0");
				if(strRet.Length > 0 && str.Length > 0)
					strRet.Append(" ").Append(currencyString).Append(" и ").Append(str).Append(" ст.");
				else
				{
					strRet.Clear();
					strRet.Append(str).Append(" ст.");
				}
			}
			else
				strRet.Append(" ").Append(currencyString);

			if(value == 0)
			{
				strRet.Clear();
				strRet.Append("0 ").Append(currencyString);
			}

			if(bMinus == true)
				strRet.Insert(0, "минус ");
			return strRet.ToString();
		}
		d = whole / 1000;
		d = Math.Round(d - Math.Floor(d), 3) * 1000;
		if(Math.Floor(d) == 1)
		{
			if(bPutAnd == true && strRet.Length > 0)
			{
				strRet.Insert(0, " и ");
				bPutAnd = false;
			}
			strRet.Insert(0, "един милион ");
		}
		else if(Math.Floor(d) != 0)
		{
			if(bPutAnd == true && strRet.Length > 0)
			{
				strRet.Insert(0, " и ");
				bPutAnd = false;
			}
			strRet.Insert(0, " милиона ").Insert(0, ToSpeech(Math.Floor(d), true));
		}
		whole /= 1000;
		whole = Math.Floor(whole);
		if(whole == 0)
		{
			currencyString = "лева";
			if(Math.Floor(value) == 1)
				currencyString = "лев";

			if(reminder > 0)
			{
				reminder *= 100;
				str = reminder.ToString("0");
				if(strRet.Length > 0 && str.Length > 0)
					strRet.Append(" ").Append(currencyString).Append(" и ").Append(str).Append(" ст.");
				else
				{
					strRet.Clear();
					strRet.Append(str).Append(" ст.");
				}
			}
			else
				strRet.Append(" ").Append(currencyString);

			if(value == 0)
			{
				strRet.Clear();
				strRet.Append("0 ").Append(currencyString);
			}

			if(bMinus == true)
				strRet.Insert(0, "минус ");
			return strRet.ToString();
		}
		d = whole / 1000;
		d = Math.Round(d - Math.Floor(d), 3) * 1000;
		if(Math.Floor(d) == 1)
			strRet.Insert(0, "един милиард ");
		else if(Math.Floor(d) != 0)
			strRet.Insert(0, " милиарда ").Insert(0, ToSpeech(Math.Floor(d), false));

		//the_end:
		currencyString = "лева";
		if(Math.Floor(value) == 1)
			currencyString = "лев";

		if(reminder > 0)
		{
			reminder *= 100;
			str = reminder.ToString("0");
			if(strRet.Length > 0 && str.Length > 0)
				strRet.Append(" ").Append(currencyString).Append(" и ").Append(str).Append(" ст.");
			else
			{
				strRet.Clear();
				strRet.Append(str).Append(" ст.");
			}
		}
		else
			strRet.Append(" ").Append(currencyString);

		if(value == 0)
		{
			strRet.Clear();
			strRet.Append("0 ").Append(currencyString);
		}

		if(bMinus == true)
			strRet.Insert(0, "минус ");
		return strRet.ToString();
	}

	private static string ToSpeech(decimal value, bool bFemale)
	{
		int reminder, reminder2, reminder3;
		var str = string.Empty;
		var str2 = string.Empty;

		if(value >= 1000)
			return str;

		reminder = ((int) value) % 10;

		switch(reminder)
		{
		case 1:
			str = bFemale ? "една" : "един";
			break;

		case 2:
			str = bFemale ? "две" : "два";
			break;

		case 3:
			str = "три";
			break;

		case 4:
			str = "четири";
			break;

		case 5:
			str = "пет";
			break;

		case 6:
			str = "шест";
			break;

		case 7:
			str = "седем";
			break;

		case 8:
			str = "осем";
			break;

		case 9:
			str = "девет";
			break;
		}

		value /= 10;
		value = Math.Floor(value);
		if(value == 0)
			return str;

		reminder2 = ((int) value) % 10;

		switch(reminder2)
		{
		case 1:
			switch(reminder)
			{
			case 0:
				str2 = "десет";
				break;

			case 1:
				str2 = "единадесет";
				break;

			case 2:
				str2 = "дванадесет";
				break;

			case 3:
				str2 = "тринадесет";
				break;

			case 4:
				str2 = "четиринадесет";
				break;

			case 5:
				str2 = "петнадесет";
				break;

			case 6:
				str2 = "шестнадесет";
				break;

			case 7:
				str2 = "седемнадесет";
				break;

			case 8:
				str2 = "осемнадесет";
				break;

			case 9:
				str2 = "деветнадесет";
				break;
			}
			break;

		case 2:
			str2 = "двадесет";
			break;

		case 3:
			str2 = "тридесет";
			break;

		case 4:
			str2 = "четиридесет";
			break;

		case 5:
			str2 = "петдесет";
			break;

		case 6:
			str2 = "шестдесет";
			break;

		case 7:
			str2 = "седемдесет";
			break;

		case 8:
			str2 = "осемдесет";
			break;

		case 9:
			str2 = "деветдесет";
			break;
		}

		if(str2.Length > 0)
		{
			if(reminder2 > 1 && str.Length > 0)
				str = str2 + " и " + str;
			else
				str = str2;
		}

		value /= 10;
		value = Math.Floor(value);
		if(value == 0)
			return str;
		reminder3 = ((int) value) % 10;

		switch(reminder3)
		{
		case 1:
			str2 = "сто";
			break;

		case 2:
			str2 = "двеста";
			break;

		case 3:
			str2 = "триста";
			break;

		case 4:
			str2 = "четиристотин";
			break;

		case 5:
			str2 = "петстотин";
			break;

		case 6:
			str2 = "шестстотин";
			break;

		case 7:
			str2 = "седемстотин";
			break;

		case 8:
			str2 = "осемстотин";
			break;

		case 9:
			str2 = "деветстотин";
			break;
		}

		if(str.Length == 0)
			str = str2;
		else
		{
			//alert('str.Length');
			if(reminder2 > 1 && reminder != 0)
				str = str2 + " " + str;
			//else str = str2 + " i " + str
			else
				str = str2 + " и " + str;
		}
		return str;
	}

	public static StringBuilder Clear(this StringBuilder sb)
	{
		sb.Remove(0, sb.Length);
		return sb;
	}

	#endregion

	public static T Clone<T, R>(this R model)
		where T : class, new()
		where R : class, T, new()
	{
		T result = new T();
		foreach(PropertyInfo prop in typeof(T)
			.GetProperties()
			.Where(q => q.CanWrite))
		{
			prop.SetValue(result, prop.GetValue(model));
		}
		return result;
	}

	public static void Init<T, R>(this R entity, T model)
		where T : class, new()
		where R : class, T, new()
	{
		foreach(PropertyInfo prop in typeof(T)
			.GetProperties()
			.Where(q => q.CanWrite))
		{
			prop.SetValue(entity, prop.GetValue(model));
		}
	}
}