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

namespace Microsoft.PartnerCenter.Connector.Context
{
    using Properties;
    using Store.PartnerCenter;
    using System;
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper class for perform Partner Center API related operations.
    /// </summary>
    public class PartnerCenterContext
    {
        private readonly SecureString _appSecret;
        private readonly SecureString _password;
        private readonly string _appId;
        private readonly string _username;

        public PartnerCenterContext(string appId, SecureString appSecret, string username, SecureString password)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }
            if (appSecret == null)
            {
                throw new ArgumentNullException(nameof(appSecret));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            _appId = appId;
            _appSecret = appSecret;
            _password = password;
            _username = username;
        }

        /// <summary>
        /// Gets an instance of <see cref="IAggregatePartner"/>.
        /// </summary>
        /// <returns>An instnace of <see cref="IAggregatePartner"/> used to interact with Partner Center.</returns>
        public IAggregatePartner GetOperations()
        {
            IPartnerCredentials credentials =
                new TokenContext(_appId, _appSecret, _username, _password)
                    .GetPartnerCenterToken($"{Settings.Default.Authority}/common");
            return PartnerService.Instance.CreatePartnerOperations(credentials);
        }

        /// <summary>
        /// Gets an instance of <see cref="IAggregatePartner"/>.
        /// </summary>
        /// <returns>An instnace of <see cref="IAggregatePartner"/> used to interact with Partner Center.</returns>
        public async Task<IAggregatePartner> GetOperationsAsync()
        {
            IPartnerCredentials credentials =
                await new TokenContext(_appId, _appSecret, _username, _password)
                    .GetPartnerCenterTokenAsync($"{Settings.Default.Authority}/common");
            return PartnerService.Instance.CreatePartnerOperations(credentials);
        }
    }
}