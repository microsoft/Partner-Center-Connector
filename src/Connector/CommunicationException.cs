/*
 * Partner Center Connector 
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

namespace Microsoft.PartnerCenter.Connector
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// User defined exception type that is thrown when an error is encountered communication with the Partner Center API.
    /// </summary>
    /// <seealso cref="Exception" />
    /// <seealso cref="ISerializable" />
    [Serializable]
    public class CommunicationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        public CommunicationException()
        {
            HttpStatusCode = HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CommunicationException(string message) : base(message)
        {
            HttpStatusCode = HttpStatusCode.BadRequest;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="httpStatusCode">The HTTP status code that was encountered with the error.</param>
        public CommunicationException(string message, HttpStatusCode httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public CommunicationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="httpStatusCode">The HTTP status code that was encountered with the error.</param>
        public CommunicationException(string message, Exception innerException, HttpStatusCode httpStatusCode)
            : base(message, innerException)
        {
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// Gets or sets the HTTP status code associated with the exception.
        /// </summary>
        /// <value>
        /// The HTTP status code that was encountered.
        /// </value>
        public HttpStatusCode HttpStatusCode
        { get; protected set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("HttpStatusCode", (int)HttpStatusCode);

            base.GetObjectData(info, context);
        }
    }
}