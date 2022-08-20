using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gcp.PubSub.Common
{
    public class TypeSerializer
    {
		public string ToJson(object obj, string comment = "", bool truncate = true)
		{
			if (obj == null)
			{
				return comment;
			}

			var dict = ToDict(obj.GetType());
			var dump = JsonSerializer.Serialize(dict);
			return !string.IsNullOrWhiteSpace(comment) ? $"{comment}{Environment.NewLine}{dump}" : dump;
		}

		public Dictionary<string, object> ToDict(Type type)
		{
			var dict = new Dictionary<string, object>();

			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				PropToJson(prop, dict);
			}

			return dict;
		}

		private void PropToJson(PropertyInfo prop, Dictionary<string, object> parent)
		{
			if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string))
			{
				if (prop.PropertyType.IsEnum)
				{
					var values = prop.PropertyType.GetEnumValues().Cast<int>();
					var names = prop.PropertyType.GetEnumNames();
					var enums = new List<string>();

					using (var enum1 = values.GetEnumerator())
					{
						var enum2 = names.GetEnumerator();

						while (enum1.MoveNext() && enum2.MoveNext())
						{
							enums.Add($"{enum2.Current}={enum1.Current}");
						}
					}

					parent[prop.Name.ToCamelCase()] = enums.ToArray();
				}
				else if (IsNullable(prop.PropertyType, out var innerType))
				{
					parent[prop.Name.ToCamelCase()] = $"{innerType.Name}|null";
				}
				else
				{
					parent[prop.Name.ToCamelCase()] = prop.PropertyType.Name;
				}
			}
			else
			{
				if (IsIEnumerable(prop.PropertyType, out var itemType))
				{
					if (itemType.IsValueType || itemType == typeof(string))
					{
						parent[prop.Name.ToCamelCase()] = new[] { itemType.Name };
					}
					else
					{
						parent[prop.Name.ToCamelCase()] = new[] { ToDict(itemType) };
					}
				}
				else
				{
					parent[prop.Name.ToCamelCase()] = ToDict(prop.PropertyType);
				}
			}
		}

		private static bool IsIEnumerable(Type type, out Type itemType)
		{
			itemType = null;

			var enumerable = type
				.GetInterfaces()
				.FirstOrDefault(x => x.IsGenericType
					&& x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			if (enumerable == null)
			{
				return false;
			}

			itemType = enumerable.GetGenericArguments()[0];

			return true;
		}

		private static bool IsNullable(Type type, out Type itemType)
		{
			itemType = Nullable.GetUnderlyingType(type);

			return itemType != null;
		}
	}
}
