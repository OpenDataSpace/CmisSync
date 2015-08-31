//-----------------------------------------------------------------------
// <copyright file="LoginException.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.Cmis.UiUtils {
    using System;
    using System.Runtime.Serialization;

    using DotCMIS.Exceptions;

    using Newtonsoft.Json;

    /// <summary>
    /// Login exception type.
    /// </summary>
    public enum LoginExceptionType {
        Unkown = int.MaxValue,
        ConnectionBroken = 9,
        NameResolutionFailure = 8,
        ServerNotFound = 7,
        HttpsSendFailure = 6,
        HttpsTrustFailure = 5,
        TargetIsNotACmisServer = 4,
        PermissionDenied = 3,
        Unauthorized = 2,
        ServerCouldNotLocateRepository = 1
    }

    /// <summary>
    /// Login exception.
    /// </summary>
    [Serializable]
    public class LoginException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.UiUtils.LoginException"/> class.
        /// </summary>
        public LoginException() : base() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.UiUtils.LoginException"/> class.
        /// </summary>
        /// <param name="inner">Inner Exception.</param>
        public LoginException(Exception inner) : base(inner.Message, inner) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception.</param>
        public LoginException(string message) : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginException"/> class.
        /// </summary>
        /// <param name="message">Message of the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public LoginException(string message, Exception inner) : base(message, inner) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginException"/> class.
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
                var ex = this.InnerException;
                var msg = ex.Message;
                if (ex == null) {
                    return LoginExceptionType.Unkown;
                } else if (ex is CmisPermissionDeniedException) {
                    return LoginExceptionType.PermissionDenied;
                } else if (ex is CmisObjectNotFoundException) {
                    return LoginExceptionType.ServerCouldNotLocateRepository;
                } else if (ex is CmisRuntimeException) {
                    if (msg == "Unauthorized") {
                        return LoginExceptionType.Unauthorized;
                    } else {
                        return LoginExceptionType.TargetIsNotACmisServer;
                    }
                } else if (ex is CmisConnectionException) {
                    return LoginExceptionType.ConnectionBroken;
                } else if (msg == "SendFailure") {
                    return LoginExceptionType.HttpsSendFailure;
                } else if (msg == "TrustFailure") {
                    return LoginExceptionType.HttpsTrustFailure;
                } else if (msg == "NameResolutionFailure") {
                    return LoginExceptionType.NameResolutionFailure;
                } else if (ex is JsonReaderException) {
                    return LoginExceptionType.TargetIsNotACmisServer;
                } else {
                    return LoginExceptionType.Unkown;
                }
            }
        } 
    }
}