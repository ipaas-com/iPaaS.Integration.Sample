using FakeStore.Data.Models;
using Integration.Abstract.Helpers;
using Integration.Abstract.Model;
using Integration.DataModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Integration.Constants;

namespace FakeStore.Data.Interface
{
    static class Identity
    {
        public static string AppName = "FakeStore";                     // rename this
    }

    public class MetaData : Integration.Abstract.MetaData
    {
        public override void LoadMetaData()
        {
            Info = new Integration.Abstract.Model.IntegrationInfo();
            Info.IntegrationFilename = String.Format("{0}.Data.dll", Identity.AppName);
            Info.IntegrationNamespace = String.Format("{0}.Data.Interface", Identity.AppName);
            Info.Name = Identity.AppName;
            Info.ApiVersion = "1.0"; // Enter the Integration API Version, not the 3rd-Party Application Version
            Info.VersionMajor = 1;
            Info.VersionMinor = 0;
            Info.VersionPatch = 0;
            Info.OAuthAuthorizationTypeId = 0; // No OAuth support

            Scopes = GetScopes();
            Presets = GetPresets();
            Tables = GetTables();
            FeatureSupport = GetFeatures();
        }

        private List<Scope> GetScopes()
        {
            Scopes = new List<Integration.Abstract.Model.Scope>();
            //FakeStore does not have web hooks, so we will not have any real scopes to use. However, we can define a polling hook for products
            Scopes.Add(new Scope() { Name = "product/poll", Description = "Poll the destination system for updated products", MappingCollectionTypeId = (int)TM_MappingCollectionType.PRODUCT, ScopeActionId = (int)ScopeAction.POLL });

            //We can also define a hook for carts and users, which can be manually triggered
            Scopes.Add(new Scope() { Name = "cart/created", Description = "Cart Created", MappingCollectionTypeId = (int)TM_MappingCollectionType.TRANSACTION, ScopeActionId = (int)ScopeAction.CREATED });
            Scopes.Add(new Scope() { Name = "user/created", Description = "User Created", MappingCollectionTypeId = (int)TM_MappingCollectionType.CUSTOMER, ScopeActionId = (int)ScopeAction.CREATED });

            return Scopes;
        }

        private List<Preset> GetPresets()
        {
            var presets = new List<Preset>();
            presets.Add(new Preset() { Name = "API Url", DataType = "string", IsRequired = true, SortOrder = 0, DefaultValue = "https://fakestoreapi.com" });
            return presets;
        }

        private List<Features> GetFeatures()
        {
            var features = new List<Features>();

            //Each model defines its own feature support, so we can just instantiate each model and call its GetFeatureSupport method
            features.Add((new Cart()).GetFeatureSupport());
            features.Add((new CartProduct()).GetFeatureSupport());
            features.Add((new Product()).GetFeatureSupport());
            features.Add((new User()).GetFeatureSupport());
            features.Add((new UserAddress()).GetFeatureSupport());

            return features;
        }

        // Enter 1 for each supported MappingCollection
        // Example: tables.Add(GenerateTableInfo("Customer", "Helptext", (int) Integration.Constants.TM_MappingCollectionType.CUSTOMER, typeof(Customer)));
        private List<TableInfo> GetTables()
        {
            var tables = new List<TableInfo>();
            tables.Add(GenerateTableInfo("Customer", "A customer record - called a User in FakeStore", (int)Integration.Constants.TM_MappingCollectionType.CUSTOMER, typeof(User)));
            tables.Add(GenerateTableInfo("CustomerAddress", "A customer address record - called a UserAddress in FakeStore", (int)Integration.Constants.TM_MappingCollectionType.CUSTOMER_ADDRESS, typeof(UserAddress)));
            tables.Add(GenerateTableInfo("Product", "A product record", (int)Integration.Constants.TM_MappingCollectionType.PRODUCT, typeof(Product)));
            tables.Add(GenerateTableInfo("Transaction", "A transaction record - called a Cart in FakeStore", (int)Integration.Constants.TM_MappingCollectionType.TRANSACTION, typeof(Cart)));
            tables.Add(GenerateTableInfo("TransactionLine", "A transaction line record - called a Cart Product in FakeStore", (int)Integration.Constants.TM_MappingCollectionType.TRANSACTION_LINE, typeof(CartProduct)));
            return tables;
        }

        /// <summary>
        /// This is a quick and dirty way to populate all fields and properties from a given class. For most fields in most classes, this will be fine,
        /// but there is no sophisticated handling for e.g. JsonIgnore'd fields. So any modifications to the standard output of this method would need to be handled
        /// manually (e.g. you could use the proc to generate a FieldInfo list, then add or remove fields as necessary)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="mappingCollectionTypeId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private TableInfo GenerateTableInfo(string name, string description, int mappingCollectionTypeId, Type type)
        {
            var table = new TableInfo() { Name = name, Description = description, MappingCollectionTypeId = mappingCollectionTypeId };
            table.Fields = new List<Integration.Abstract.Model.FieldInfo>();


            var members = new List<System.Reflection.MemberInfo>();
            members.AddRange(type.GetProperties());
            members.AddRange(type.GetFields());

            foreach (var member in members)
            {
                //Do not use iPaaS ignore fields
                if (member.IsDefined(typeof(iPaaSMetaDataAttribute), true))
                    continue;

                var fieldInfo = new Integration.Abstract.Model.FieldInfo
                {
                    Name = member.Name,
                    Type = GetFieldType(member) //Add the initial guess at the field type
                };

                if (member.IsDefined(typeof(iPaaSMetaDataAttribute), true))
                {
                    // Check for iPaaSMetaData attribute. Copy the description, required, and type, if present
                    var meta = member.GetCustomAttribute<iPaaSMetaDataAttribute>();
                    if (meta != null)
                    {
                        fieldInfo.Description = meta.Description;
                        if (meta.HasRequired) //We have special flags to indicate whether this was set or not
                            fieldInfo.Required = meta.Required;
                        if (meta.HasType)//We have special flags to indicate whether this was set or not
                            fieldInfo.Type = meta.Type.ToString().ToLower();
                    }
                }

                table.Fields.Add(fieldInfo);
            }
            return table;
        }

        // <summary>
        /// This only provides a generic guess at the data type. It will correctly identify int, long, bool, DateTime, Guid, and string types. All other types will be returned as "none"
        /// Manually adjust as necessary after generating the FieldInfo list. For example, a password or date field must be manually adjusted.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        private string GetFieldType(MemberInfo memberInfo)
        {
            //Convert the memberInfo int SY_DataType
            Type type;
            if (memberInfo is PropertyInfo property)
                type = property.PropertyType;
            else
                type = ((System.Reflection.FieldInfo)memberInfo).FieldType;

            if (type == typeof(int) || type == typeof(int?) || type == typeof(long) || type == typeof(long?))
                return "number";
            if (type == typeof(bool) || type == typeof(bool?))
                return "bool";
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return "datetime";
            if (type == typeof(Guid) || type == typeof(Guid?))
                return "guid";
            if (type == typeof(string))
                return "string";
            return "none";
        }
    }
}
