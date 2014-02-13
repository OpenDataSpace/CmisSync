using System;
using System.IO;

namespace CmisSync.Lib.Events
{

    ///
    ///<summary>Events Created By ContentChange Eventhandler<summary>
    ///
    public class ContentChangeEvent : ISyncEvent
    {
        public DotCMIS.Enums.ChangeType Type { get; private set; }

        public string ObjectId { get; private set; }

        public ContentChangeEvent (DotCMIS.Enums.ChangeType type, string objectId)
        {
            if (objectId == null) {
                throw new ArgumentNullException ("Argument null in ContenChangeEvent Constructor", "path");
            }
            if (type == null) {
                throw new ArgumentNullException ("Argument null in ContenChangeEvent Constructor", "type");
            }
            Type = type;
            ObjectId = objectId;
        }

        public override string ToString ()
        {
            return string.Format ("ContenChangeEvent with type \"{0}\" and ID \"{1}\"", Type, ObjectId);
        }

    }
}

