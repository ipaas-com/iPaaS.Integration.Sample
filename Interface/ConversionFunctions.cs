using FakeStore.Data.Models;
using Integration.Data.IPaaSApi;
using Integration.Data.IPaaSApi.Model;
using Integration.Data.Utilities;
using Integration.DataModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Integration.Constants;

namespace FakeStore.Data.Interface
{
    // Static methods can be used for performing routine transformations that are specific to the external system.
    // Methods provided here can be accessed by the subscriber from within their UI portal when building mappings.
    public class ConversionFunctions : Integration.Abstract.ConversionFunctions 
    {
        [ThreadStatic]
        private static IPaaSApiCallWrapper _iPaaSApiCallWrapper;
        public static IPaaSApiCallWrapper iPaaSApiCallWrapper
        {
            get
            {
                if (_iPaaSApiCallWrapper == null)
                    _iPaaSApiCallWrapper = new IPaaSApiCallWrapper();
                _iPaaSApiCallWrapper.EstablishConnection(ContextConnection, ContextConnection.Settings);
                return _iPaaSApiCallWrapper;
            }
        }

        private static new Connection ContextConnection { get { return (Connection)Integration.Abstract.ConversionFunctions.ContextConnection; } }
        private static Settings IntegrationSettings { get { return (Settings)ContextConnection.Settings; } }
        private static CallWrapper IntegrationCallWrapper { get { return (CallWrapper)ContextConnection.CallWrapper; } }

        /// <summary>
        /// Determine the total price of the items in the cart
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        public static async Task<decimal> CalculateCartTotalAsync(List<CartProduct> products)
        {
            var total = 0m;
            if(products == null || products.Count == 0)
                return total;

            foreach(var product in products)
                total += await CalculateCartProductTotalAsync(product.ProductId, product.Quantity);

            return total;
        }

        /// <summary>
        /// Determine the total price of a single item and scale it for quantity
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static async Task<decimal> CalculateCartProductTotalAsync(int productId, int quantity)
        {
            var iProduct = new Product();
            var fullProduct = (Product)await iProduct.Get(IntegrationCallWrapper, productId);
            var total = fullProduct.Price * quantity;
            return total;
        }
    }
}
