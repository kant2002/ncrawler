using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace NCrawler.Extensions
{
	public static class ObjectExtensions
	{
		#region Class Methods

		/// <summary>
		/// Dynamically retrieves a property value.
		/// </summary>
		/// <typeparam name="T">The expected return data type</typeparam>
		/// <param name="obj">The object to perform on.</param>
		/// <param name="propertyName">The Name of the property.</param>
		/// <param name="defaultValue">The default value to return.</param>
		/// <returns>The property value.</returns>
		/// <example>
		/// <code>
		/// var type = Type.GetType("System.IO.FileInfo, mscorlib");
		/// var file = type.CreateInstance(@"c:\autoexec.bat");
		/// if(file.GetPropertyValue&lt;bool&gt;("Exists")) {
		///  var reader = file.InvokeMethod&lt;StreamReader&gt;("OpenText");
		///  Console.WriteLine(reader.ReadToEnd());
		///  reader.Close();
		/// }
		/// </code>
		/// </example>
		public static T GetPropertyValue<T>(this object obj, string propertyName, T defaultValue)
		{
			Type type = obj.GetType();
#if !PORTABLE
            PropertyInfo property = type.GetProperty(propertyName);
#else
            PropertyInfo property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
#endif

            if (property.IsNull())
			{
				return defaultValue;
			}

			object value = property.GetValue(obj, null);
			return value is T ? (T) value : defaultValue;
		}

		/// <summary>
		/// Dynamically sets a property value.
		/// </summary>
		/// <param name="obj">The object to perform on.</param>
		/// <param name="propertyName">The Name of the property.</param>
		/// <param name="value">The value to be set.</param>
		public static void SetPropertyValue(this object obj, string propertyName, object value)
		{
			Type type = obj.GetType();
#if !PORTABLE
            PropertyInfo property = type.GetProperty(propertyName);
#else
            PropertyInfo property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
#endif

            if (!property.IsNull())
			{
				property.SetValue(obj, value, null);
			}
		}

		public static bool IsNull<T>(this T @object)
		{
			return Equals(@object, null);
		}

		public static bool IsIn<T>(this T t, params T[] tt)
		{
			return tt.Contains(t);
		}

        #endregion
        public static string ToJson<T>(this T o) where T : class, new()
        {
            var serializer = new JsonSerializer();            
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, o, typeof(T));
                return writer.ToString();
            }
        }

        public static T FromJson<T>(this string data) where T : class, new()
        {
            var serializer = new JsonSerializer();
            using (var reader = new StringReader(data))
            {
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }
    }
}