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
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class Communication
    {
        /// <summary>
        /// Sends a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="requestUri">The Uri where the request should be sent.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="token">The access token value used to authorize the request.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// requestUri
        /// or
        /// mediaType
        /// or
        /// token
        /// </exception>
        /// <exception cref="CommunicationException"></exception>
        public async Task<T> GetAsync<T>(string requestUri, MediaTypeWithQualityHeaderValue mediaType, string token)
        {
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri));
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Add the required headers for the request.
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(mediaType);

                    response = await client.GetAsync(requestUri);

                    if (!response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        throw new CommunicationException(result, response.StatusCode);
                    }

                    return await response.Content.ReadAsAsync<T>();
                }
            }
            finally
            {
                response = null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="requestUri">The Uri where the request should be sent.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="token">The access token value used to authorize the request.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// requestUri
        /// or
        /// mediaType
        /// or
        /// token
        /// </exception>
        /// <exception cref="CommunicationException"></exception>
        public async Task<string> GetStringAsync<T>(string requestUri, MediaTypeWithQualityHeaderValue mediaType, string token)
        {
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri));
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Add the required headers for the request.
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(mediaType);

                    response = await client.GetAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    throw new CommunicationException(result, response.StatusCode);
                }
            }
            finally
            {
                response = null;
            }
        }

        /// <summary>
        /// Sends a POST request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="requestUri">The Uri where the request should be sent.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// requestUri
        /// or
        /// content
        /// </exception>
        /// <exception cref="CommunicationException"></exception>
        public async Task<T> PostAsync<T>(string requestUri, HttpContent content)
        {
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri));
            }
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    response = await client.PostAsync(requestUri, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        throw new CommunicationException(result, response.StatusCode);
                    }

                    return await response.Content.ReadAsAsync<T>();
                }
            }
            finally
            {
                response = null;
            }
        }

        /// <summary>
        /// Sends a POST request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="requestUri">The Uri where the request should be sent.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        /// <param name="token">The access token value used to authorize the request.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// requestUri
        /// or
        /// mediaType
        /// or
        /// content
        /// or
        /// token
        /// </exception>
        /// <exception cref="CommunicationException"></exception>
        public async Task<T> PostAsync<T>(string requestUri, MediaTypeWithQualityHeaderValue mediaType, HttpContent content, string token)
        {
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri));
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(mediaType);

                    response = await client.PostAsync(requestUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<T>();
                    }

                    string result = await response.Content.ReadAsStringAsync();
                    throw new CommunicationException(result, response.StatusCode);
                }
            }
            finally
            {
                response = null;
            }
        }

        /// <summary>
        /// Asynchronously posts the content as JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="content">The content.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// requestUri
        /// or
        /// mediaType
        /// or
        /// content
        /// or
        /// token
        /// </exception>
        /// <exception cref="CommunicationException"></exception>
        public async Task<T> PostAsJsonAsync<T>(string requestUri, MediaTypeWithQualityHeaderValue mediaType, T content, string token)
        {
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri));
            }
            if (mediaType == null)
            {
                throw new ArgumentNullException(nameof(mediaType));
            }
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(mediaType);

                    response = await client.PostAsJsonAsync(requestUri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<T>();
                    }

                    string result = await response.Content.ReadAsStringAsync();
                    throw new CommunicationException(result, response.StatusCode);
                }
            }
            finally
            {
                response = null;
            }
        }
    }
}