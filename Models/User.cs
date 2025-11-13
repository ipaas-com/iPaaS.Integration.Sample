using FakeStore.Data.Interface;
using Integration;
using Integration.Abstract.Helpers;
using Integration.Abstract.Model;
using Integration.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Integration.Abstract.Constants;
using static Integration.Constants;

namespace FakeStore.Data.Models
{
    public class User : AbstractIntegrationData
    {
        //Build a model based on this JSON structure above. Each feild should in Pascal case with a JsonProperty decorator in the original case
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("name")]
        public UserFullName Name { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("address")]
        public UserAddress Address { get; set; }

        public User() 
        { 
            Name = new UserFullName();
        }

        public void AssignUserIdToAddress()
        {
            if(Address != null)
                Address.UserId = Id;
        }

        public override async Task<object> Get(CallWrapper activeCallWrapper, object id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/users/" + Convert.ToString(id), $"User_GET(id: {id})",
                $"LOAD User ({id})", typeof(User), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.CUSTOMER, RestSharp.Method.Get);
            var output = (User)await apiCall.ProcessRequestAsync();
            output.AssignUserIdToAddress();
            return output;
        }

        public override async Task<object> Create(CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/users", $"User_POST(Email: {Email})",
                $"CREATE User ({Email})", typeof(User), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.CUSTOMER, RestSharp.Method.Post);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Create.Body", JsonConvert.SerializeObject(this));
            var output = (User)await apiCall.ProcessRequestAsync();
            output.AssignUserIdToAddress();
            return output;
        }

        public override async Task<object> Update(CallWrapper activeCallWrapper)
        {
            var apiCall = new APICall(activeCallWrapper, $"/users/{Id}", $"User_PUT(Id: {Id})",
                $"UPDATE User ({Id})", typeof(User), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.CUSTOMER, RestSharp.Method.Put);
            apiCall.AddBodyParameter(this);
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName} Update.Body", JsonConvert.SerializeObject(this));
            var output = (User)await apiCall.ProcessRequestAsync();
            output.AssignUserIdToAddress();
            return output;
        }

        public override async Task<object> Delete(CallWrapper activeCallWrapper, object _id)
        {
            var apiCall = new APICall(activeCallWrapper, $"/users/{_id}", $"User_DELETE(Id: {_id})",
                $"DELETE User ({_id})", typeof(User), activeCallWrapper?.TrackingGuid,
                Constants.TM_MappingCollectionType.CUSTOMER, RestSharp.Method.Delete);
            var output = (User)await apiCall.ProcessRequestAsync();
            return output;
        }

        public override async Task<List<BulkTransferRequest>> Poll(Interface.CallWrapper activeCallWrapper, string filter)
        {
            activeCallWrapper._integrationConnection.Logger.Log_Technical("D", $"{Identity.AppName}.User.Poll", "Call to User.Poll, but there is currently no functionality for this method.");

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
            retVal.MappingCollectionType = (int)TM_MappingCollectionType.CUSTOMER;
            retVal.MappingDirectionId = (int)TM_MappingDirection.BIDIRECTIONAL;
            retVal.Support = Integration.Abstract.Model.Features.SupportLevel.Full;
            retVal.AdditionalInformation = "FakeStore User entity supports full CRUD operations.";
            retVal.AllowInitialization = false;

            retVal.CollisionHandlingSupported = false;
            retVal.CustomfieldSupported = false;
            retVal.IndependentTransferSupported = true;
            retVal.PollingSupported = false;
            retVal.RecordMatchingSupported = false;
            retVal.ExternalWebhookSupportId = (int)WH_ExternalSupport.NONE;

            retVal.SupportedEndpoints.Add(new FeatureSupportEndpoint() { Value = "/users/{Id}", Note = "" });

            retVal.ExternalIdFormats.Add(new ExternalIdFormat() { RecordExternalIdFormat = "{{Id}}" });

            retVal.ExternalDataTypes.Add(new FeatureSupportDataType() { Value = "User", Note = "The User table" });

            retVal.SupportedMethods.Add((int)TM_SyncType.ADD);
            retVal.SupportedMethods.Add((int)TM_SyncType.UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.ADD_AND_UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE_TRIGGERED_UPDATE);

            return retVal;
        }
    }

    public class UserAddress
    {
        //Any model that we have mappings to must have a primary key. This model will be mapped to the mapping collection CUSTOMER_ADDRESS, so we must have a way to determine its primary
        //key. Since there is nothing unique in the model itself, we create this field that we populate based on the parent User. Since each User only has one address, we can just use the
        //UserId as a unique ID.
        [JsonIgnore]
        public int UserId { get; set; }

        [JsonProperty("geolocation")]
        public UserAddressGeoLocation GeoLocation { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("zipcode")]
        public string ZipCode { get; set; }

        public Features GetFeatureSupport()
        {

            var retVal = new Features();
            retVal.MappingCollectionType = (int)TM_MappingCollectionType.CUSTOMER_ADDRESS;
            retVal.MappingDirectionId = (int)TM_MappingDirection.BIDIRECTIONAL;
            retVal.Support = Integration.Abstract.Model.Features.SupportLevel.Full;
            retVal.AdditionalInformation = "FakeStore User Address entity supports full CRUD operations.";
            retVal.AllowInitialization = false;

            retVal.CollisionHandlingSupported = false;
            retVal.CustomfieldSupported = false;
            retVal.IndependentTransferSupported = false; //Transfers only occur as part of User transfers
            retVal.PollingSupported = false;
            retVal.RecordMatchingSupported = false;
            retVal.ExternalWebhookSupportId = (int)WH_ExternalSupport.NONE;

            //No direct endpoint for addresses
            //retVal.SupportedEndpoints.Add(new FeatureSupportEndpoint() { Value = "/users/{Id}", Note = "" });

            //No unique id for addresses
            //retVal.ExternalIdFormats.Add(new ExternalIdFormat() { RecordExternalIdFormat = "{{Id}}" });

            retVal.ExternalDataTypes.Add(new FeatureSupportDataType() { Value = "User Address", Note = "The User Address object contained within the User object" });

            retVal.SupportedMethods.Add((int)TM_SyncType.ADD);
            retVal.SupportedMethods.Add((int)TM_SyncType.UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.ADD_AND_UPDATE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE);
            retVal.SupportedMethods.Add((int)TM_SyncType.DELETE_TRIGGERED_UPDATE);

            return retVal;
        }
    }

    public class UserAddressGeoLocation
    {
        [JsonProperty("lat")]
        public string Lat{ get; set; }

        [JsonProperty("long")]
        public string Long { get; set; }
    }

    public class UserFullName
    {
        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

    }
}
