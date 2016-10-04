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
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Represents an authornization token.
    /// </summary>
    public class AuthorizationToken
    {
        private long _expiresIn;

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [JsonProperty("access_token")]
        public string AccessToken
        { get; private set; }

        /// <summary>
        /// Determines whether or the access is near expiration.
        /// </summary>
        /// <returns><c>true</c> if near expiration; otherwise <c>false</c>.</returns>
        public bool IsNearExpiry()
        {
            return DateTime.UtcNow > ExpiresOn.AddMinutes(-1);
        }

        /// <summary>
        /// Gets the point in time in which the Access Token returned in the AccessToken property ceases to be valid.
        /// </summary>
        /// <value>
        /// The point in time when the Access Token ceases to be valid.
        /// </value>
        public DateTime ExpiresOn => DateTime.UtcNow.AddSeconds(_expiresIn);

        [JsonProperty("expires_in")]
        private long ExpiresIn
        {
            get { return _expiresIn; }
            set { _expiresIn = value; }
        }
    }
}
