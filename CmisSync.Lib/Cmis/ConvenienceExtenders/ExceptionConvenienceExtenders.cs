
namespace CmisSync.Lib.Cmis.ConvenienceExtenders {
    using System;

    using DotCMIS.Exceptions;

    public static class ExceptionConvenienceExtenders {
        public static bool IsVirusDetectionException(this CmisConstraintException ex) {
            if (ex.ErrorContent.Contains("Virus")) {
                return true;
            } else {
                return false;
            }
        }
    }
}