using System.Collections.Generic;

namespace CmisSync.Lib.Storage
{
    /// <summary>
    /// Extended attribute reader interface
    /// </summary>
    public interface IExtendedAttributeReader
    {
        /// <summary>
        /// Gets the extended attribute.
        /// </summary>
        /// <returns>
        /// The extended attribute.
        /// </returns>
        /// <param name='path'>
        /// Path.
        /// </param>
        /// <param name='key'>
        /// Key.
        /// </param>
        string GetExtendedAttribute(string path, string key);
        /// <summary>
        /// Sets the extended attribute.
        /// </summary>
        /// <param name='path'>
        /// Path.
        /// </param>
        /// <param name='key'>
        /// Key.
        /// </param>
        /// <param name='value'>
        /// Value.
        /// </param>
        void SetExtendedAttribute(string path, string key, string value);
        /// <summary>
        /// Removes the extended attribute.
        /// </summary>
        /// <param name='path'>
        /// Path.
        /// </param>
        /// <param name='key'>
        /// Key.
        /// </param>
        void RemoveExtendedAttribute(string path, string key);
        /// <summary>
        /// Lists the attribute keys.
        /// </summary>
        /// <returns>
        /// The attribute keys.
        /// </returns>
        /// <param name='path'>
        /// Path.
        /// </param>
        List<string> ListAttributeKeys(string path);
    }

    /// <summary>
    /// Wrong platform exception should be thrown if the executing platfom
    /// is not the target platform of the compilation
    /// </summary>
    public class WrongPlatformException : System.Exception {}
}
