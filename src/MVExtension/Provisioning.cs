/*
 * Partner Center MV Extension 
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

namespace Microsoft.PartnerCenter.MVExtension
{
    using MetadirectoryServices;
    using System;
    using System.Linq;
    using System.Xml.Linq;

    public class Provisioning : IMVSynchronization
    {
        private const long ADS_UF_ACCOUNTDISABLE = 0x2;
        private string _password;
        private string _root;
        private string _users;

        /// <summary>
        /// Initializes the rule extension object.
        /// </summary>
        /// <remarks>
        /// If an exception occurs in this method, the IMASynchronization.Terminate method is not called. 
        /// If the IMASynchronization.Terminate method releases any resources allocated in the initialize method, 
        /// those resources remain when an exception occurs in this method because the IMASynchronization.Terminate 
        /// method is not called. Release any resources allocated in this method as part of your exception handling routine.
        /// </remarks>
        public void Initialize()
        {
            XDocument doc;

            try
            {
                doc = XDocument.Load($@"{Utils.ExtensionsDirectory}\settings.xml");

                var settings = doc.Descendants("rules-extension-properties")
                    .Descendants("account-provisioning")
                    .Descendants("container").Select(node => new
                    {
                        Password = node.Element("password").Value,
                        Root = node.Element("root").Value,
                        Users = node.Element("users").Value
                    }).Single();

                _password = settings.Password;
                _root = settings.Root;
                _users = settings.Users;
            }
            finally
            {
                doc = null;
            }
        }

        /// <summary>
        /// Evaluates connected objects in response to changes to a metaverse object.
        /// </summary>
        /// <param name="mventry"></param>
        /// <remarks>The synchronization service calls this method during a management agent run.</remarks>
        public void Provision(MVEntry mventry)
        {
            ConnectedMA agent;
            int connectors;

            try
            {
                agent = mventry.ConnectedMAs["ADMA"];
                connectors = agent.Connectors.Count;

                if (connectors != 0)
                {
                    return;
                }

                if (mventry.ObjectType.Equals("customer", StringComparison.CurrentCultureIgnoreCase))
                {
                    ProvisionCustomer(agent, mventry);
                }
                if (mventry.ObjectType.Equals("person", StringComparison.CurrentCultureIgnoreCase))
                {
                    ProvisionPerson(agent, mventry);
                }
            }
            finally
            {
                agent = null;
            }
        }

        /// <summary>
        /// Determines if the metaverse object should be deleted along with the connector space object 
        /// after a connector space object has been disconnected from a metaverse object during inbound 
        /// synchronization.
        /// </summary>
        /// <param name="csentry"></param>
        /// <param name="mventry"></param>
        /// <returns>
        /// <c>true</c> if the metaverse entry should be deleted; otherwise <c>false</c>.
        /// </returns>
        public bool ShouldDeleteFromMV(CSEntry csentry, MVEntry mventry)
        {
            throw new EntryPointNotImplementedException();
        }

        /// <summary>
        /// This method is used to free resources owned by the rules extension.
        /// </summary>
        /// <remarks>
        /// The synchronization service calls this method the rules extension object is no longer needed.
        /// </remarks>
        public void Terminate()
        { }

        private void ProvisionCustomer(ConnectedMA agent, MVEntry mventry)
        {
            CSEntry csentry;
            ReferenceValue dn;

            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }
            if (mventry == null)
            {
                throw new ArgumentNullException(nameof(mventry));
            }

            try
            {
                /* Define the appropriate provisioning logic here. */
            }
            finally
            {
                csentry = null;
                dn = null;
            }
        }

        private void ProvisionPerson(ConnectedMA agent, MVEntry mventry)
        {
            CSEntry csentry;
            ReferenceValue dn;
            string accountName;
            string basePath;
            string companyName;

            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }
            if (mventry == null)
            {
                throw new ArgumentNullException(nameof(mventry));
            }

            try
            {
                accountName = mventry["userPrincipalName"].Value.Split('@')[0];
                basePath = $"{_users},OU={mventry["company"].Value},{_root}";
                companyName = mventry["company"].Value.Replace(" ", string.Empty);

                dn = agent.CreateDN(
                    $"CN={mventry["displayName"].Value},{basePath}");
                csentry = agent.Connectors.StartNewConnector("user");
                csentry.DN = dn;
                csentry["company"].Value = mventry["company"].Value;
                csentry["displayName"].Value = mventry["displayName"].Value;
                csentry["givenName"].Value = mventry["firstName"].Value;
                csentry["sAMAccountName"].Value = $"{accountName}_{companyName}";
                csentry["sn"].Value = mventry["lastName"].Value;
                csentry["unicodePwd"].Value = _password;
                csentry["userAccountControl"].IntegerValue = ADS_UF_ACCOUNTDISABLE;
                csentry["userPrincipalName"].Value = mventry["userPrincipalName"].Value;
                csentry.CommitNewConnector();
            }
            finally
            {
                csentry = null;
                dn = null;
            }
        }
    }
}