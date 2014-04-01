using System.Collections.Generic;

namespace CmisSync.Lib.Storage
{
    /// <summary>
    /// Extended attribute reader interface
    /// </summary>
    public interface IExtendedAttributeReader
    {
        string GetExtendedAttribute(string path, string key);

        void SetExtendedAttribute(string path, string key, string value);

        void RemoveExtendedAttribute(string path, string key);

        List<string> ListAttributeKeys(string path);
    }

}
