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
        using IdentityModel.Clients.ActiveDirectory;
        using Models;
        using Properties;
        using Store.PartnerCenter;
        using Store.PartnerCenter.Extensions;
        using System;
        using System.Collections.Generic;
        using System.Net.Http;
        using System.Runtime.InteropServices;
        using System.Security;
        using System.Threading.Tasks;

        /// <summary>
        /// Helper class for retrieving access tokens.
        /// </summary>
        public sealed class TokenContext
        {
            private readonly SecureString _appSecret;
            private readonly SecureString _password;
            private readonly string _appId;
            private readonly string _username;

            public TokenContext(string appId, SecureString appSecret, string username, SecureString password)
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
            /// Gets an access token from the authority.
            /// </summary>
            /// <param name="authority">Address of the authority to issue the token.</param>
            /// <param name="resource">Identifier of the target resource that is the recipent of the requested token.</param>
            /// <returns>An instnace of <see cref="AuthenticationResult"/> that represented the access token.</returns>
            /// <exception cref="ArgumentNullException">
            /// authority
            /// or
            /// resource
            /// </exception>
            public AuthenticationResult GetAppOnlyToken(string authority, string resource)
            {
                if (string.IsNullOrEmpty(authority))
                {
                    throw new ArgumentNullException(nameof(authority));
                }
                if (string.IsNullOrEmpty(resource))
                {
                    throw new ArgumentNullException(nameof(resource));
                }

                return SynchronousExecute(() => GetAppOnlyTokenAsync(authority, resource));
            }

            /// <summary>
            /// Gets an access token from the authority.
            /// </summary>
            /// <param name="authority">Address of the authority to issue the token.</param>
            /// <param name="resource">Identifier of the target resource that is the recipent of the requested token.</param>
            /// <returns>An instnace of <see cref="AuthenticationResult"/> that represented the access token.</returns>
            /// <exception cref="ArgumentNullException">
            /// authority
            /// or
            /// resource
            /// </exception>
            public async Task<AuthenticationResult> GetAppOnlyTokenAsync(string authority, string resource)
            {
                AuthenticationContext authContext;

                if (string.IsNullOrEmpty(authority))
                {
                    throw new ArgumentNullException(nameof(authority));
                }
                if (string.IsNullOrEmpty(resource))
                {
                    throw new ArgumentNullException(nameof(resource));
                }

                try
                {
                    authContext = new AuthenticationContext(authority);

                    return await authContext.AcquireTokenAsync(
                        resource,
                        new ClientCredential(
                            _appId,
                            _appSecret));
                }
                finally
                {
                    authContext = null;
                }
            }

            /// <summary>
            /// Gets an access token from the authority.
            /// </summary>
            /// <param name="authority">Address of the authority to issue the token.</param>
            /// <param name="resource">Identifier of the target resource that is the recipent of the requested token.</param>
            /// <returns>An instnace of <see cref="AuthenticationResult"/> that represented the access token.</returns>
            /// <exception cref="ArgumentNullException">
            /// authority
            /// or
            /// resource
            /// </exception>
            public AuthorizationToken GetAppUserToken(string authority, string resource)
            {
                if (string.IsNullOrEmpty(authority))
                {
                    throw new ArgumentNullException(nameof(authority));
                }
                if (string.IsNullOrEmpty(resource))
                {
                    throw new ArgumentNullException(nameof(resource));
                }

                return SynchronousExecute(() => GetAppPlusUserTokenAsync(authority, resource));
            }

            /// <summary>
            /// Gets an access token from the authority.
            /// </summary>
            /// <param name="authority">Address of the authority to issue the token.</param>
            /// <param name="resource">Identifier of the target resource that is the recipent of the requested token.</param>
            /// <returns>An instnace of <see cref="AuthenticationResult"/> that represented the access token.</returns>
            /// <exception cref="ArgumentNullException">
            /// authority
            /// or
            /// resource
            /// </exception>
            public async Task<AuthorizationToken> GetAppPlusUserTokenAsync(string authority, string resource)
            {
                if (string.IsNullOrEmpty(authority))
                {
                    throw new ArgumentNullException(nameof(authority));
                }
                if (string.IsNullOrEmpty(resource))
                {
                    throw new ArgumentNullException(nameof(resource));
                }

                Communication comm;
                List<KeyValuePair<string, string>> values;

                try
                {
                    comm = new Communication();
                    values = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", _appId),
                        new KeyValuePair<string, string>("client_secret", ConvertToUnsecureString(_appSecret)),
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("password", ConvertToUnsecureString(_password)),
                        new KeyValuePair<string, string>("resource", resource),
                        new KeyValuePair<string, string>("username", _username),
                    };

                    using (HttpContent content = new FormUrlEncodedContent(values))
                    {
                        return await comm.PostAsync<AuthorizationToken>(
                            $"{Settings.Default.Authority}/common/oauth2/token", content);
                    }
                }
                finally
                {
                    comm = null;
                    values = null;
                }
            }

            /// <summary>
            /// Get an access token for the Partner Center API.
            /// </summary>
            /// <param name="authority">Address of the authority to issue the token.</param>
            /// <returns>
            /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// authority
            /// </exception>
            public IPartnerCredentials GetPartnerCenterToken(string authority)
            {

                if (string.IsNullOrEmpty(authority))
                {
                    throw new ArgumentNullException(nameof(authority));
                }

                return SynchronousExecute(() => GetPartnerCenterTokenAsync(authority));
            }

            /// <summary>
            /// Get an access token for the Partner Center API.
            /// </summary>
            /// <param name="authority">Address of the authority to issue the token.</param>
            /// <returns>
            /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// authority
            /// </exception>
            public async Task<IPartnerCredentials> GetPartnerCenterTokenAsync(string authority)
            {
                AuthorizationToken token;
                IPartnerCredentials credentials;

                if (string.IsNullOrEmpty(authority))
                {
                    throw new ArgumentNullException(nameof(authority));
                }

                try
                {
                    token = await GetAppPlusUserTokenAsync(
                        authority,
                        "https://api.partnercenter.microsoft.com");

                    credentials = await PartnerCredentials.Instance.GenerateByUserCredentialsAsync(
                        _appId,
                        new AuthenticationToken(token.AccessToken, token.ExpiresOn));

                    return credentials;
                }
                finally
                {
                    token = null;
                }
            }

            private static string Key => "Resource:PartnerCenterAPI";

            /// <summary>
            /// Synchronously executes an asynchronous function.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="operation">The operation.</param>
            /// <returns></returns>
            private static T SynchronousExecute<T>(Func<Task<T>> operation)
            {
                try
                {
                    return Task.Run(async () => await operation()).Result;
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerException;
                }
            }

            private static string ConvertToUnsecureString(SecureString securePassword)
            {
                if (securePassword == null)
                {
                    throw new ArgumentNullException(nameof(securePassword));
                }

                IntPtr unmanagedString = IntPtr.Zero;

                try
                {
                    unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                    return Marshal.PtrToStringUni(unmanagedString);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }
        }
    }