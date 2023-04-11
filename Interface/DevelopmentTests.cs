using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeStore.Data.Models;
using Newtonsoft.Json;
using static Integration.Constants;

namespace FakeStore.Data.Interface
{
    public class DevelopmentTests
    {
        // Example development test running Connection Validation
        public static async Task Integration_Wrapper_ValidateConnection(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var response = await wrapper.ValidateConnection();

            // Test with successful and invalid credentials.
            // Are you handling Error responses correctly?
            Console.WriteLine($"Connection validation response {response}");
        }

        #region Product CRUD tests

        public static async Task Product_Get(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var iProduct = new Product();
            var product = (Product)await iProduct.Get(wrapper, 5);

            if (product != null)
                Console.WriteLine($"Retrieved product {product.Title}");
            else
                Console.WriteLine($"Failed to retrieve product");
        }

        public static async Task Product_Update(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var iProduct = new Product();
            var product = (Product)await iProduct.Get(wrapper, 5);

            if (product != null)
                Console.WriteLine($"Retrieved product {product.Title}");
            else
                Console.WriteLine($"Failed to retrieve product");

            var oldPrice = product.Price;
            product.Price = product.Price + .01M;

            var newProduct = (Product)await product.Update(wrapper);

            Console.WriteLine($"Updated product price from {oldPrice} to {newProduct.Price}");
        }

        public static async Task Product_Create(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var product = new Product();
            product.Title = "Adams Tight Lies 2 Wood";
            product.Price = 348.99M;
            product.Description = "Adams Tight Lies 2 Wood";
            product.Category = "electronics";

            var newProduct = (Product)await product.Create(wrapper);

            Console.WriteLine($"Created product with ID {newProduct.Id}");
        }

        public static async Task Product_Delete(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var iProduct = new Product();
            await iProduct.Delete(wrapper, 5);

            Console.WriteLine($"Delete product with ID 5");
        }
        #endregion

        #region Cart CRUD tests
        public static async Task Cart_Get(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var iCart = new Cart();
            var cart = (Cart)await iCart.Get(wrapper, 5);

            if (cart != null)
                Console.WriteLine($"Retrieved cart {cart.Id}");
            else
                Console.WriteLine($"Failed to retrieve cart");
        }

        public static async Task Cart_Update(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var iCart = new Cart();
            var cart = (Cart)await iCart.Get(wrapper, 5);
            if (cart != null)
                Console.WriteLine($"Retrieved cart {cart.Id}");
            else
                Console.WriteLine($"Failed to retrieve cart");
            var oldUser = cart.UserId;
            cart.UserId = 1;
            var newCart = (Cart)await cart.Update(wrapper);
            Console.WriteLine($"Updated cart total from {oldUser} to {newCart.UserId}");
        }

        public static async Task Cart_Create(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var cart = new Cart();
            cart.UserId = 5;
            cart.Products.Add(new CartProduct() { ProductId = 7, Quantity = 2 });
            cart.Products.Add(new CartProduct() { ProductId = 8, Quantity = 1 });
            var newCart = (Cart)await cart.Create(wrapper);
            Console.WriteLine($"Created cart with ID {newCart.Id}");
        }

        public static async Task Cart_Delete(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var iCart = new Cart();
            await iCart.Delete(wrapper, 5);
            Console.WriteLine($"Delete cart with ID 5");
        }
        #endregion

        #region User CRUD tests

        public static async Task User_Get(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var iUser = new User();
            var user = (User)await iUser.Get(wrapper, 5);

            if (user != null)
                Console.WriteLine($"Retrieved user {user.Name.FirstName} {user.Name.LastName}");
            else
                Console.WriteLine($"Failed to retrieve usert");
        }

        public static async Task User_Update(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var iUser = new User();
            var user = (User)await iUser.Get(wrapper, 5);
            if (user != null)
                Console.WriteLine($"Retrieved user {user.Name.FirstName} {user.Name.LastName}");
            else
                Console.WriteLine($"Failed to retrieve user");
            var oldEmail = user.Email;
            user.Email = "derek@yahoo.com";
            var newUser = (User)await user.Update(wrapper);
            Console.WriteLine($"Updated cart total from {oldEmail} to {newUser.Email}");
        }

        public static async Task User_Create(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var user = new User();
            user.Name.FirstName = "John";
            user.Name.LastName = "Doe";
            user.Email = "john.doe@gmail.com";
            user.Phone = "555-123-4141";

            var newUser = (User)await user.Create(wrapper);
            Console.WriteLine($"Created user with ID {newUser.Id}");
        }

        public static async Task User_Delete(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;
            var iUser = new User();
            await iUser.Delete(wrapper, 5);
            Console.WriteLine($"Delete user with ID 5");
        }
        #endregion

        public static async Task Test_iPaaSCallWrapper(Integration.Abstract.Connection connection)
        {
            var conn = (Connection)connection;
            var wrapper = conn.CallWrapper;

            var resp = await conn.IPaasApiCallWrapper.LookupIPaaSId_GETAsync(Integration.Data.IPaaSApi.IPaaSApiCallWrapper.EndpointURL.Customers, "5", (int)TM_MappingCollectionType.CUSTOMER, 10684);
            Console.WriteLine($"Response from LookupIPaaSId_GETAsync for Customer 5: {resp}");
        }
    }
}
