
namespace CmisSync.Lib.Cmis.UiUtils {
    using System;
    using System.Runtime.Serialization;

    using DotCMIS.Exceptions;

    public enum LoginExceptionType {
        Unkown,
        PermissionDenied,
        NameResolutionFailure,
        ServerNotFound,
        ConnectionBroken,
        TargetIsNotACmisServer,
        ServerCouldNotLocateRepository,
        HttpsSendFailure,
        HttpsTrustFailure
    }

    [Serializable]
    public class LoginException : Exception{
        public LoginException() : base(typeof(LoginException).Name) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception.</param>
        public LoginException(string message) : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public LoginException(string message, Exception inner) : base(message, inner) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisPermissionDeniedException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected LoginException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        /// <summary>
        /// Gets the type of the exception based on the inner exception.
        /// </summary>
        /// <value>The type.</value>
        public LoginExceptionType Type {
            get {
                Exception ex = this.InnerException;
                if (ex == null) {
                    return LoginExceptionType.Unkown;
                } else if (ex is CmisPermissionDeniedException) {
                    return LoginExceptionType.PermissionDenied;
                } else if (ex is CmisObjectNotFoundException) {
                    return LoginExceptionType.ServerCouldNotLocateRepository;
                } else if (ex is CmisRuntimeException) {
                    return LoginExceptionType.TargetIsNotACmisServer;
                } else if (ex is CmisConnectionException) {
                    return LoginExceptionType.ConnectionBroken;
                } else if (ex.Message == "SendFailure") {
                    return LoginExceptionType.HttpsSendFailure;
                } else if (ex.Message == "TrustFailure") {
                    return LoginExceptionType.HttpsTrustFailure;
                } else if (ex.Message == "NameResolutionFailure") {
                    return LoginExceptionType.NameResolutionFailure;
                } else {
                    return LoginExceptionType.Unkown;
                }
            }
        } 
    }
}