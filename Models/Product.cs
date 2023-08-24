using FakeStore.Data.Interface;
using Integration;
using Integration.Abstract.Helpers;
using Integration.Abstract.Model;
using Integration.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public override async Task<List<BulkTransferRequest>> Poll(CallWrapper activeCallWrapper, string filter)
        {
            var apiCall = new APICall(activeCallWrapper, $"/products", $"Products_GET()",
                $"POLL Products", typeof(List<Product>), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.PRODUCT, RestSharp.Method.Get);

            var initalOutput = (List<Product>)await apiCall.ProcessRequestAsync();

            //The API call above gets ALL products, but this polling procedure should only return the NEW products. Since we don't have many filter
            //options on the Fakestore API, we have to get all the products, then filter here. 

            //In order to only return products that have not already been returned via previous POLL requests, we store the highestProductIdPolled.
            //Since FakeStore products are stored with an incrementing id, we can determine which products have not been previously polled by checkign to see
            //if they are higher than the previous highestProductIdPolled value. 
            int highestProductIdPolled = 0;
            var highestProductIdPolledPD = activeCallWrapper._integrationConnection.Settings.PersistentData.GetValue("highestProductIdPolled");

            //If the highestProductIdPolled value from PersistentData exists and is a valid int, that is our starting point. Otherwise, we use the default value of 0
            if (highestProductIdPolledPD != null && int.TryParse(Convert.ToString(highestProductIdPolledPD.Value), out int highestProductIdPolledPDValueInt))
                highestProductIdPolled = highestProductIdPolledPDValueInt;
            else
                highestProductIdPolledPD = new PersistentData() { Name = "highestProductIdPolled" };

            //Filter the output to just those products where the id is higher than the highestProductIdPolled
            var outputProducts = initalOutput.FindAll(x => x.Id > highestProductIdPolled);

            //Now that we have our new list of polled products, we need to update the highestProductIdPolled value in PersistentData to match the id of
            //the highest Id our output. If there are no new products (e.g. otherProducts.Count == 0) then there is nothing to save. 
            if(outputProducts.Count > 0)
            {
                highestProductIdPolledPD.Value = outputProducts[outputProducts.Count - 1].Id;
                activeCallWrapper._integrationConnection.Settings.PersistentData.SaveValue(highestProductIdPolledPD);
            }

            var output = new List<BulkTransferRequest>();

            foreach (var product in outputProducts)
                output.Add(new BulkTransferRequest(Convert.ToString(product.Id)));

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
