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
        /// <summary>
        /// An unkown exception happend.
        /// </summary>
        Unkown = int.MaxValue,

        /// <summary>
        /// The connection is broken.
        /// </summary>
        ConnectionBroken = 9,

        /// <summary>
        /// The name could not be resolved.
        /// </summary>
        NameResolutionFailure = 8,

        /// <summary>
        /// The server could not be found.
        /// </summary>
        ServerNotFound = 7,

        /// <summary>
        /// The https send failure.
        /// </summary>
        HttpsSendFailure = 6,

        /// <summary>
        /// The https trust failure.
        /// </summary>
        HttpsTrustFailure = 5,

        /// <summary>
        /// The target is not a cmis server.
        /// </summary>
        TargetIsNotACmisServer = 4,

        /// <summary>
        /// The permission is denied. Perhaps wrong credentials are passed.
        /// </summary>
        PermissionDenied = 3,

        /// <summary>
        /// Unauthorized exception occured.
        /// </summary>
        Unauthorized = 2,

        /// <summary>
        /// The server could not locate a repository.
        /// </summary>
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
        public LoginException(Exception inner) : base(inner != null ? inner.Message : "LoginException", inner) {
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