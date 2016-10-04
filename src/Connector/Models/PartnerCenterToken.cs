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

namespace Microsoft.PartnerCenter.Connector.Models
{
    using Store.PartnerCenter;
    using System;

    /// <summary>
    /// Partner Center token object used for serialization purposes.
    /// </summary>
    /// <seealso cref="IPartnerCredentials" />
    public class PartnerCenterToken : IPartnerCredentials
    {
        /// <summary>
        /// Gets the expiry time in UTC for the token.
        /// </summary>
        public DateTimeOffset ExpiresAt
        { get; set; }

        /// <summary>
        /// Gets the token needed to authenticate with the partner API service.
        /// </summary>
        public string PartnerServiceToken
        { get; set; }

        /// <summary>
        /// Indicates whether the partner credentials have expired or not.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if credentials have expired; otherwise <c>false</c>.
        /// </returns>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAt;
        }
    }
}