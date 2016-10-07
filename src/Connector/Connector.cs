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
    using Context;
    using MetadirectoryServices;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Enumerators;
    using Store.PartnerCenter.Exceptions;
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Customers;
    using Store.PartnerCenter.Models.Query;
    using Store.PartnerCenter.Models.Users;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class Connector : IMAExtensible2CallImport, IMAExtensible2GetCapabilities,
        IMAExtensible2GetParameters, IMAExtensible2GetSchema
    {
        private Dictionary<string, string> _customersInfo;
        private Dictionary<string, string>.Enumerator _customersInfoEnumerator;
        private IAggregatePartner _operations;
        private IResourceCollectionEnumerator<SeekBasedResourceCollection<Customer>> _customersEnumerator;
        private IResourceCollectionEnumerator<SeekBasedResourceCollection<CustomerUser>> _usersEnumerator;
        private SeekBasedResourceCollection<Customer> _customers;
        private SeekBasedResourceCollection<CustomerUser> _users;
        private bool _setupCustomerIdsEnumerator;
        private bool _setupUsersEnumerators;

        /// <summary>
        /// Default size of the import page.
        /// </summary>
        public int ImportDefaultPageSize => 25;

        /// <summary>
        /// Maximum size of the import page.
        /// </summary>
        public int ImportMaxPageSize => 50;

        /// <summary>
        /// Gets the capabilities of the management agent.
        /// </summary>
        /// <value>
        /// The capabilities of the management agent.
        /// </value>
        public MACapabilities Capabilities => new MACapabilities
        {
            ConcurrentOperation = true,
            DeleteAddAsReplace = true,
            DeltaImport = true,
            DistinguishedNameStyle = MADistinguishedNameStyle.Generic,
            ExportPasswordInFirstPass = false,
            ExportType = MAExportType.AttributeReplace,
            FullExport = false,
            NoReferenceValuesInFirstExport = true,
            Normalizations = MANormalizations.None,
            ObjectConfirmation = MAObjectConfirmation.Normal,
            ObjectRename = false
        };

        /// <summary>
        /// Used by the management agent to allow for code cleanup. 
        /// </summary>
        /// <param name="importRunStep"></param>
        /// <returns></returns>
        public CloseImportConnectionResults CloseImportConnection(CloseImportConnectionRunStep importRunStep)
        {
            _customersEnumerator = null;
            _customersInfo = null;
            _customers = null;
            _users = null;
            _usersEnumerator = null;

            _customersInfoEnumerator.Dispose();

            return new CloseImportConnectionResults();
        }

        /// <summary>
        /// Get an array of values indicating the configuration parameter definitions supported by the 
        /// management agent. This method is called to display the parameters user interface page for 
        /// configuring Connectivity, Global, Partitions, and Run-Step parameters.
        /// </summary>
        /// <param name="configParameters">A collection of <see cref="ConfigParameter"/> objects.</param>
        /// <param name="page">The <see cref="ConfigParameterPage"/> which contains the parameters.</param>
        /// <returns>A list of child <see cref="ConfigParameterDefinition"/> objects.</returns>
        public IList<ConfigParameterDefinition> GetConfigParameters(
            KeyedCollection<string, ConfigParameter> configParameters, ConfigParameterPage page)
        {
            List<ConfigParameterDefinition> parameterDefinitionList = new List<ConfigParameterDefinition>();

            if (page == ConfigParameterPage.Connectivity)
            {
                parameterDefinitionList.Add(
                    ConfigParameterDefinition.CreateStringParameter("AppId", string.Empty));
                parameterDefinitionList.Add(
                    ConfigParameterDefinition.CreateEncryptedStringParameter("AppSecret", string.Empty));
                parameterDefinitionList.Add(
                    ConfigParameterDefinition.CreateStringParameter("Username", string.Empty));
                parameterDefinitionList.Add(
                    ConfigParameterDefinition.CreateEncryptedStringParameter("Password", string.Empty));
            }

            return parameterDefinitionList;
        }

        /// <summary>
        /// Persists a batch of entries in the connected system. Called for multiple entries that are imported. 
        /// </summary>
        /// <param name="importRunStep">A <see cref="GetImportEntriesRunStep"/> object that contains import information.</param>
        /// <returns>
        /// An instance of<see cref="GetImportEntriesResults"/> that contains custom data, whether there are more objects
        /// to import, and a list of <see cref="GetImportEntriesRunStep"/> objects.
        /// </returns>
        public GetImportEntriesResults GetImportEntries(GetImportEntriesRunStep importRunStep)
        {
            if (_customersEnumerator.HasValue)
            {
                return GetCustomerImportEntries();
            }
            if (_setupCustomerIdsEnumerator)
            {
                _customersInfoEnumerator = _customersInfo.GetEnumerator();
                _customersInfoEnumerator.MoveNext();
                _setupCustomerIdsEnumerator = false;
            }
            if (!_setupUsersEnumerators)
            {
                return GetUserImportEntries();
            }

            ConfigureUserEnumerator();

            return GetUserImportEntries();
        }

        /// <summary>
        /// Gets the schema for the connected system (Partner Center).
        /// </summary>
        /// <param name="configParameters">The configuration parameters.</param>
        /// <returns></returns>
        public Schema GetSchema(KeyedCollection<string, ConfigParameter> configParameters)
        {
            Schema schema = Schema.Create();

            schema.Types.Add(GetCustomerType());
            schema.Types.Add(GetPersonType());

            return schema;
        }

        /// <summary>
        /// Used to configure the import session and is called once at the beginning of the import.
        /// </summary>
        /// <param name="configParameters">A collection of <see cref="ConfigParameter"/> objects.</param>
        /// <param name="types">Contains a <see cref="Schema"/> that defines the management agent's schema</param>
        /// <param name="importRunStep">Contains an <see cref="OpenImportConnectionRunStep"/> object.</param>
        /// <returns></returns>
        public OpenImportConnectionResults OpenImportConnection(KeyedCollection<string, ConfigParameter> configParameters,
            Schema types, OpenImportConnectionRunStep importRunStep)
        {
            InitializeConnector(configParameters);

            _setupCustomerIdsEnumerator = true;
            _setupUsersEnumerators = true;

            // This dictionary will contain a complete list of all customer identifiers that are processed. These
            // identifiers will be used to obtain the users that belong to the customers that were processed.
            _customersInfo = new Dictionary<string, string>();
            // Obtain the collection of customer broken out by page size. This is required because the synchronization 
            // will only import a set size of entries at once. The page size is controlled by the ImportDefaultPageSize
            // and ImportMaxPageSize properties. 
            _customers = _operations.Customers.Query(QueryFactory.Instance.BuildIndexedQuery(ImportDefaultPageSize));
            // Create a customer enumerator which will be used to traverse the pages of customers.
            _customersEnumerator = _operations.Enumerators.Customers.Create(_customers);

            return new OpenImportConnectionResults();
        }

        /// <summary>
        /// Validates the configurations parameters.
        /// </summary>
        /// <param name="configParameters">Contains a collection of <see cref="ConfigParameter"/> objects.</param>
        /// <param name="page">The <see cref="ConfigParameterPage"/> which contains the parameters</param>
        /// <returns>An aptly populated instance of <see cref="ParameterValidationResult"/>.</returns>
        public ParameterValidationResult ValidateConfigParameters(KeyedCollection<string, ConfigParameter> configParameters, ConfigParameterPage page)
        {
            InitializeConnector(configParameters);

            return new ParameterValidationResult();
        }

        private CSEntryChange GetCsEntryChange<T>(T item, string dn, string objectType, Dictionary<string, string> extraAttributes = null)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (string.IsNullOrEmpty(dn))
            {
                throw new ArgumentNullException(nameof(dn));
            }
            if (string.IsNullOrEmpty(objectType))
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            CSEntryChange csEntryChange = CSEntryChange.Create();

            csEntryChange.DN = dn;
            csEntryChange.ObjectModificationType = ObjectModificationType.Add;
            csEntryChange.ObjectType = objectType;

            csEntryChange.AttributeChanges.Add(
                AttributeChange.CreateAttributeAdd("objectID", dn));

            if (extraAttributes != null)
            {
                foreach (var element in extraAttributes)
                {
                    csEntryChange.AttributeChanges.Add(
                        AttributeChange.CreateAttributeAdd(element.Key, element.Value));
                }
            }

            AddAttributeChanges(ref csEntryChange, item);

            return csEntryChange;
        }

        private CSEntryChange GetCsEntryChange<T, V>(T item1, V item2, string dn, string objectType)
        {
            if (item1 == null)
            {
                throw new ArgumentNullException(nameof(item1));
            }
            if (item2 == null)
            {
                throw new ArgumentNullException(nameof(item2));
            }
            if (string.IsNullOrEmpty(dn))
            {
                throw new ArgumentNullException(nameof(dn));
            }
            if (string.IsNullOrEmpty(objectType))
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            CSEntryChange csEntryChange = CSEntryChange.Create();

            csEntryChange.DN = dn;
            csEntryChange.ObjectModificationType = ObjectModificationType.Add;
            csEntryChange.ObjectType = objectType;

            csEntryChange.AttributeChanges.Add(
                AttributeChange.CreateAttributeAdd("objectID", dn));

            AddAttributeChanges(ref csEntryChange, item1);
            AddAttributeChanges(ref csEntryChange, item2);

            return csEntryChange;
        }

        private GetImportEntriesResults GetCustomerImportEntries()
        {
            List<CSEntryChange> csentries = new List<CSEntryChange>();
            bool moreToImport;

            // Add the connector space objects for customers obtained using the Partner Center API.
            csentries.AddRange(
                _customersEnumerator.Current.Items.Select(c => GetCsEntryChange(c, c.CompanyProfile, c.Id, "customer")));
            // Add all of the customer identifiers from the current page of records to the collection.
            foreach (Customer c in _customersEnumerator.Current.Items)
            {
                _customersInfo.Add(c.CompanyProfile.CompanyName, c.Id);
            }
            // Move to the next page of customers. 
            _customersEnumerator.Next();

            moreToImport = _setupCustomerIdsEnumerator || _customersEnumerator.HasValue;

            return new GetImportEntriesResults(string.Empty, moreToImport, csentries);
        }

        private static SchemaType GetCustomerType()
        {
            SchemaType customerType = SchemaType.Create("customer", false);

            customerType.Attributes.Add(SchemaAttribute.CreateAnchorAttribute("objectID", AttributeType.String));
            customerType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("companyName", AttributeType.String));
            customerType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("domain", AttributeType.String));
            customerType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("tenantId", AttributeType.String));

            return customerType;
        }

        private static SchemaType GetPersonType()
        {
            SchemaType personType = SchemaType.Create("user", false);

            personType.Attributes.Add(SchemaAttribute.CreateAnchorAttribute("objectID", AttributeType.String));
            personType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("company", AttributeType.String));
            personType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("displayName", AttributeType.String));
            personType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("firstName", AttributeType.String));
            personType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("lastName", AttributeType.String));
            personType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("usageLocation", AttributeType.String));
            personType.Attributes.Add(SchemaAttribute.CreateSingleValuedAttribute("userPrincipalName", AttributeType.String));

            return personType;
        }

        private GetImportEntriesResults GetUserImportEntries()
        {
            Dictionary<string, string> extraAttributes = new Dictionary<string, string>();
            List<CSEntryChange> csentries = new List<CSEntryChange>();
            bool moreToImport = false;

            extraAttributes.Add("company",
                _customersInfoEnumerator.Current.Key);

            csentries.AddRange(_usersEnumerator.Current.Items.Select(u => GetCsEntryChange(u, u.Id, "user", extraAttributes)));

            _usersEnumerator.Next();

            if (_usersEnumerator.HasValue)
            {
                moreToImport = true;
            }
            else
            {
                if (_customersInfoEnumerator.MoveNext())
                {
                    _setupUsersEnumerators = true;
                    moreToImport = true;
                }
            }

            return new GetImportEntriesResults(string.Empty, moreToImport, csentries);
        }

        private void AddAttributeChanges<T>(ref CSEntryChange csEntryChange, T item)
        {
            if (csEntryChange == null)
            {
                throw new ArgumentNullException(nameof(csEntryChange));
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.CanRead && x.CanWrite)
                .Where(x => x.PropertyType == typeof(string))
                .Where(x => x.GetGetMethod(true).IsPublic);

            string name, value;

            foreach (PropertyInfo info in properties)
            {
                value = info.GetValue(item, null) as string;

                if (string.IsNullOrEmpty(value) || info.Name.Equals("Id", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                name = $"{char.ToLower(info.Name[0])}{info.Name.Substring(1)}";
                csEntryChange.AttributeChanges.Add(
                    AttributeChange.CreateAttributeAdd(name, info.GetValue(item, null)));
            }
        }

        private void ConfigureUserEnumerator()
        {
            try
            {
                // Obtain the collection of users broken out by page size. This is required because the synchronization 
                // will only import a set size of entries at once. The page size is controlled by the ImportDefaultPageSize
                // and ImportMaxPageSize properties.
                _users = _operations.Customers.ById(_customersInfoEnumerator.Current.Value)
                    .Users.Query(QueryFactory.Instance.BuildIndexedQuery(ImportDefaultPageSize));
                // Create a customer user enumerator which will be used to traverse the pages of customer users.
                _usersEnumerator = _operations.Enumerators.CustomerUsers.Create(_users);

                _setupUsersEnumerators = false;
            }
            catch (PartnerException)
            {
                _customersInfoEnumerator.MoveNext();
                ConfigureUserEnumerator();
            }
        }

        private void InitializeConnector(KeyedCollection<string, ConfigParameter> configParameters)
        {
            SecureString appSecret = GetEncryptedParameterValue(configParameters, "AppSecret");
            SecureString password = GetEncryptedParameterValue(configParameters, "Password");
            string appId = GetParameterValue(configParameters, "AppId");
            string username = GetParameterValue(configParameters, "Username");

            if (_operations == null)
            {
                _operations = new PartnerCenterContext(
                    appId, appSecret, username, password).GetOperations();
            }
        }

        internal static string GetParameterValue(KeyedCollection<string, ConfigParameter> configParameters,
            string keyName)
        {
            if (configParameters.Contains(keyName) && !configParameters[keyName].IsEncrypted)
            {
                return configParameters[keyName].Value;
            }

            throw new ExtensibleExtensionException($"Expected parameter was not found: {keyName}");
        }

        internal static SecureString GetEncryptedParameterValue(KeyedCollection<string, ConfigParameter> configParameters,
            string keyName)
        {
            if (configParameters.Contains(keyName) && configParameters[keyName].IsEncrypted)
            {
                return configParameters[keyName].SecureValue;
            }

            throw new ExtensibleExtensionException($"Expected encrypted parameter was not found: {keyName}");
        }
    }
}