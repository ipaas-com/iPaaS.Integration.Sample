using FakeStore.Data.Interface;
using Integration;
using Integration.Abstract;
using Integration.Abstract.Helpers;
using Integration.Abstract.Model;
using Integration.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Integration.Abstract.Constants;
using static Integration.Constants;

namespace FakeStore.Data.Models
{
    public class Cart : AbstractIntegrationData
    {
        [JsonProperty("id")]
        [iPaaSMetaData(Required = false, Type = SY_DataType.NUMBER)]
        public int Id { get; set; }

        [JsonProperty("userId")]
        [iPaaSMetaData(Required = true, Type = SY_DataType.NUMBER)]
        public int UserId { get; set; }

        [JsonProperty("date")]
        [iPaaSMetaData(Required = true, Type = SY_DataType.DATE)]
        public DateTime Date { get; set; }

        [JsonProperty("products")]
        public List<CartProduct> Products { get; set; }

        public Cart()
        {
            Products = new List<CartProduct>();
        }

        public void AssignCartIdToProducts()
        {
            if (Products != null)
                foreach(var product in Products)
                    product.CartId = Id;
        }

        public override async Task<object> Get(Interface.CallWrapper activeCallWrapper, object id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts/" + Convert.ToString(id), $"Cart_GET(id: {id})",
                $"LOAD Cart ({id})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Integration.Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Get);

            var output = (Cart)await apiCall.ProcessRequestAsync();
            output.AssignCartIdToProducts();
            return output;
        }

        public override async Task<object> Create(Interface.CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts", $"Cart_POST(User: {UserId})",
                $"CREATE Cart ({UserId})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Integration.Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Post);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Create.Body", JsonConvert.SerializeObject(this));
            var output = (Cart)await apiCall.ProcessRequestAsync();
            output.AssignCartIdToProducts();
            return output;
        }

        public override async Task<object> Update(Interface.CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts/{Id}", $"Cart_PUT(Id: {Id})",
                $"UPDATE Cart ({Id})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Integration.Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Put);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Update.Body", JsonConvert.SerializeObject(this));
            var output = (Cart)await apiCall.ProcessRequestAsync();
            output.AssignCartIdToProducts();
            return output;
        }

        public override async Task<object> Delete(Interface.CallWrapper activeCallWrapper, object _id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts/{_id}", $"Cart_DELETE(Id: {_id})",
                $"DELETE Cart ({_id})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Integration.Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Delete);
            var output = (Cart)await apiCall.ProcessRequestAsync();
            return output;
        }

        public override async Task<List<BulkTransferRequest>> Poll(Interface.CallWrapper activeCallWrapper, string filter)
        {
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName}.Cart.Poll", "Call to Cart.Poll, but there is currently no functionality for this method.");

            var output = new List<BulkTransferRequest>();
            return output;
        }

        public override object GetPrimaryId()
        {
            return Id;
        }

        public override void SetPrimaryId(string PrimaryId, bool ThrowErrorOnInvalid = false)
        {
            Id = int.Parse(PrimaryId);
        }

        public new Features GetFeatureSupport()
        {

            var retVal = new Features();
            retVal.MappingCollectionType = (int)TM_MappingCollectionType.TRANSACTION;
            retVal.MappingDirectionId = (int)TM_MappingDirection.BIDIRECTIONAL;
            retVal.Support = Integration.Abstract.Model.Features.SupportLevel.Full;
            retVal.AdditionalInformation = "FakeStore Cart entity supports full CRUD operations.";
            retVal.AllowInitialization = false;

            retVal.CollisionHandlingSupported = false;
            retVal.CustomfieldSupported = false;
            retVal.IndependentTransferSupported = true;
            retVal.PollingSupported = true;
            retVal.RecordMatchingSupported = false;
            retVal.ExternalWebhookSupportId = (int)WH_ExternalSupport.LOGICAL_SUPPORT_FOR_POLLING;

            retVal.SupportedEndpoints.Add(new FeatureSupportEndpoint() { Value = "/carts/{Id}", Note = "" });

            retVal.ExternalIdFormats.Add(new ExternalIdFormat() { RecordExternalIdFormat = "{{Id}}" });

            retVal.ExternalDataTypes.Add(new FeatureSupportDataType() { Value = "Cart", Note = "The Cart table" });

            retVal.SupportedMethods.Add((int)TM_SyncType.ADD);
            retVal.SupportedMethods.Add((int)TM_SyncType.UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.ADD_AND_UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE_TRIGGERED_UPDATE);

            return retVal;
        }
    }

    public class CartProduct
    {
        //Any model that we have mappings to must have a primary key. This model will be mapped to the mapping collection TRANSACTION_LINE, so we must have a way to determine its primary
        //key. Since there is nothing unique in the model itself, we create this field that we populate based on the parent cart, then append a bar and the product id. This will give
        //us a unique reference to this line item entry.
        [JsonIgnore]
        public int CartId { get; set; }

        [JsonProperty("productId")]
        [iPaaSMetaData(Required = true, Type = SY_DataType.NUMBER)]
        public int ProductId { get; set; }

        [Obsolete]
        [JsonProperty("quantity")]
        [iPaaSMetaData(Required = false, Type = SY_DataType.NUMBER)]
        public int Quantity { get; set; }

        [JsonProperty("title")]
        [iPaaSMetaData(Required = true, Type = SY_DataType.STRING)]
        public string Title { get; set; }

        [JsonProperty("price")]
        [iPaaSMetaData(Required = true, Type = SY_DataType.NUMBER)]
        public decimal? Price { get; set; }

        [JsonProperty("description")]
        [iPaaSMetaData(Required = false, Type = SY_DataType.STRING)]
        public string Description { get; set; }

        [JsonProperty("category")]
        [iPaaSMetaData(Required = false, Type = SY_DataType.STRING)]
        public string Category { get; set; }

        [JsonProperty("image")]
        [iPaaSMetaData(Required = true, Type = SY_DataType.STRING)]
        public string Image { get; set; }



        internal string GetPrimaryId()
        {
            return $"{CartId}|{ProductId}";
        }

        public new Features GetFeatureSupport()
        {

            var retVal = new Features();
            retVal.MappingCollectionType = (int)TM_MappingCollectionType.TRANSACTION_LINE;
            retVal.MappingDirectionId = (int)TM_MappingDirection.BIDIRECTIONAL;
            retVal.Support = Integration.Abstract.Model.Features.SupportLevel.Full;
            retVal.AdditionalInformation = "FakeStore Cart Product entity supports full CRUD operations via the parent Cart entity.";
            retVal.AllowInitialization = false;

            retVal.CollisionHandlingSupported = false;
            retVal.CustomfieldSupported = false;
            retVal.IndependentTransferSupported = false;
            retVal.PollingSupported = true;
            retVal.RecordMatchingSupported = false;
            retVal.ExternalWebhookSupportId = (int)WH_ExternalSupport.LOGICAL_SUPPORT_FOR_POLLING;

            //Support is handled via the parent Cart endpoint
            //retVal.SupportedEndpoints.Add(new FeatureSupportEndpoint() { Value = "/carts/{Id}", Note = "" });

            retVal.ExternalIdFormats.Add(new ExternalIdFormat() { RecordExternalIdFormat = "{{Id}}" });

            retVal.ExternalDataTypes.Add(new FeatureSupportDataType() { Value = "Cart Product", Note = "The Cart Product table" });

            retVal.SupportedMethods.Add((int)TM_SyncType.ADD);
            retVal.SupportedMethods.Add((int)TM_SyncType.UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.ADD_AND_UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE_TRIGGERED_UPDATE);

            return retVal;
        }
    }
}
