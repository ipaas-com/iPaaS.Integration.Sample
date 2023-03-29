using FakeStore.Data.Interface;
using Integration;
using Integration.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStore.Data.Models
{
    public class Product : AbstractIntegrationData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("rating")]
        public ProductRating Rating { get; set; }


        public override async Task<object> Get(CallWrapper activeCallWrapper, object id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/products/" + Convert.ToString(id), $"Product_GET(id: {id})",
                $"LOAD Product ({id})", typeof(Product), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.PRODUCT, RestSharp.Method.Get);

            var output = (Product)await apiCall.ProcessRequestAsync();
            return output;
        }

        public override async Task<object> Create(CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/products", $"Product_POST(Title: {Title})",
                $"CREATE Product ({Title})", typeof(Product), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.PRODUCT, RestSharp.Method.Post);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Create.Body", JsonConvert.SerializeObject(this));
            var output = (Product)await apiCall.ProcessRequestAsync();
            return output;
        }

        public override async Task<object> Update(CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/products/{Id}", $"Product_PUT(Id: {Id})",
                $"UPDATE Product ({Id})", typeof(Product), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.PRODUCT, RestSharp.Method.Put);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Update.Body", JsonConvert.SerializeObject(this));
            var output = (Product)await apiCall.ProcessRequestAsync();
            return output;
        }

        public override async Task<object> Delete(CallWrapper activeCallWrapper, object id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/products/{id}", $"Product_DELETE(Id: {id})",
                $"DELETE Product ({id})", typeof(Product), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.PRODUCT, RestSharp.Method.Delete);
            var output = (Product)await apiCall.ProcessRequestAsync();
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

    public class ProductRating
    {
        [JsonProperty("rate")]
        public decimal Rate { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
