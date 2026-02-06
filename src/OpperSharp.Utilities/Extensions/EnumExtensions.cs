using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace OpperSharp.Utilities.Extensions
{
	public static class EnumExtensions
	{
		// Note that we never need to expire these cache items, so we just use ConcurrentDictionary rather than MemoryCache
		private static readonly ConcurrentDictionary<string, string> DisplayNameCache = new ConcurrentDictionary<string, string>();

		public static string GetDescription(this Enum value)
		{
			if (value == null)
			{
				return string.Empty;
			}

			var key = $"{value.GetType().FullName}.{value}";

			var displayName = DisplayNameCache.GetOrAdd(key, x =>
			{
				var fieldInfo = value
					.GetType()
					.GetTypeInfo()
					.GetField(value.ToString());

				if (fieldInfo == null)
				{
					return value.ToString();
				}

				var name = (DescriptionAttribute[])fieldInfo
					.GetCustomAttributes(typeof(DescriptionAttribute), false);

				return name.Length > 0 ? name[0].Description : value.ToString();
			});

			return displayName;
		}
	}
}
