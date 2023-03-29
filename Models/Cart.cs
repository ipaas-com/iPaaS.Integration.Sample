using FakeStore.Data.Interface;
using Integration;
using Integration.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FakeStore.Data.Models
{
    public class Cart : AbstractIntegrationData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("date")]
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

        public override async Task<object> Get(CallWrapper activeCallWrapper, object id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts/" + Convert.ToString(id), $"Cart_GET(id: {id})",
                $"LOAD Cart ({id})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Get);

            var output = (Cart)await apiCall.ProcessRequestAsync();
            output.AssignCartIdToProducts();
            return output;
        }

        public override async Task<object> Create(CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts", $"Cart_POST(User: {UserId})",
                $"CREATE Cart ({UserId})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Post);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Create.Body", JsonConvert.SerializeObject(this));
            var output = (Cart)await apiCall.ProcessRequestAsync();
            output.AssignCartIdToProducts();
            return output;
        }

        public override async Task<object> Update(CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts/{Id}", $"Cart_PUT(Id: {Id})",
                $"UPDATE Cart ({Id})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Put);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Update.Body", JsonConvert.SerializeObject(this));
            var output = (Cart)await apiCall.ProcessRequestAsync();
            output.AssignCartIdToProducts();
            return output;
        }

        public override async Task<object> Delete(CallWrapper activeCallWrapper, object _id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/carts/{_id}", $"Cart_DELETE(Id: {_id})",
                $"DELETE Cart ({_id})", typeof(Cart), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.TRANSACTION, RestSharp.Method.Delete);
            var output = (Cart)await apiCall.ProcessRequestAsync();
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

    }

    public class CartProduct
    {
        //Any model that we have mappings to must have a primary key. This model will be mapped to the mapping collection TRANSACTION_LINE, so we must have a way to determine its primary
        //key. Since there is nothing unique in the model itself, we create this field that we populate based on the parent cart, then append a bar and the product id. This will give
        //us a unique reference to this line item entry.
        [JsonIgnore]
        public int CartId { get; set; }

        [JsonProperty("productId")]
        public int ProductId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        internal string GetPrimaryId()
        {
            return $"{CartId}|{ProductId}";
        }
    }
}
