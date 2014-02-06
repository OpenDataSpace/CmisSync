using System;

using DotCMIS.Exceptions;

namespace CmisSync.Lib.Events
{
    public class PermissionDeniedEvent : ExceptionEvent
    {
        public PermissionDeniedEvent (DotCMIS.Exceptions.CmisPermissionDeniedException e) : base(e)
        {}
    }

}
