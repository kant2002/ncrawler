// -----------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Andrey Kurdiumov">
// Copyright (c) Andrey Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace NCrawler.EntityFramework
{
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// Extension to object.
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Convert object to binary data.
        /// </summary>
        /// <typeparam name="T">Type of the object to save.</typeparam>
        /// <param name="o">Object instance to save.</param>
        /// <returns>Serialized object instance.</returns>
        public static byte[] ToBinary<T>(this T o) where T : class, new()
        {
            var dc = new DataContractSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                dc.WriteObject(ms, o);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserialize instance of given type from specified byte sequence.
        /// </summary>
        /// <typeparam name="T">Type of data to deserialize.</typeparam>
        /// <param name="byteArray">Serialized data.</param>
        /// <returns>Deserialized object instance.</returns>
        public static T FromBinary<T>(this byte[] byteArray) where T : class, new()
        {
            var dc = new DataContractSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                ms.Write(byteArray, 0, byteArray.Length);
                ms.Seek(0, SeekOrigin.Begin);
                return dc.ReadObject(ms) as T;
            }
        }
    }
}