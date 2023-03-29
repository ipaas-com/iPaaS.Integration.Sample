# iPaaS.com Integration Sample

This project contains examples of how to implement methods throughout your new Integration.  It should be used for reference only.

It is recommended to start with a new integration project.  If you have not done so yet, please review [iPaaS.com Integration Template](https://github.com/ipaas-com/iPaaS.Integration.Template) to get started.

# FakeStore POS
## Overview
In order to provide a semi-real world example of how an integration will be coded, implemented, and deployed, we used the Template project as a basis to connect to a sample API that includes a simple set of endpoints. These endpoints return hardcoded data and do not have long term changes from updates, creations, or deletions. But nevertheless the API provides a useful example of what your integration project will need to include and the steps you will need to follow as you build it.

## How To Use This Project
You can use this project in two ways:
  1. As an example of how an integration will be implemented using the Template project
  2. As a sample project that you can compile, modify, upload, and execute as needed to try different features of the iPaaS integration system.

## How This Project Was Created
This project was created entirely using the Template project as a basis.

### Register
Before we get started with the code, we need to register the integration in the Integration Experience.

### Create VisualStudio Project.
The first step with the code is to create a project in VisualStudio. I created a project with the Class Library template using .NET Core 3.1.

Once I created the project, I copied the code files from the Templare project.

Then I added the necessary NuGet packages. Our FakeStore API uses Rest, so we need to add the version of iPaaS that is supported by iPaaS. As of this writing that is 18.0.3. So the full set of packages are:
  1. iPaaS.Integration.SDK (as of this writing, using version 1.0.4-CI-20230322-214338)
  2. Newtonsoft.Json (13.0.3)
  3. RestSharp (108.0.3)
  4. RestSharp.Serializers.NewtonsoftJson (108.0.3)

### Create Models
The next step is to create your data models. The template includes a suggested way to ease data access and meet the requirements - an abstract class called AbstractIntegrationData. If we build our models to inherit this class, we will get some built in handling later on via the template settings in TranslationUtilities. Note that this exact structure is not required, but is merely a suggestion to easily allow all of your models to meet the iPaaS requirements.

In the case of FakeStore, there are only a few models that we need to create: Products, Carts, and Users. A normal integration would have separate endpoints for Carts and Orders (or Transactions), but in the case of FakeStore, we only have Carts, so we will treat those as if they are completed transactions. The models were created in the Models folder.

Once I added the fields to the models, I moved on to populating the override methods required by AbstractIntegrationData. The Get, Create, Update, and Delete methods control the interface between the model and the API. The PrimaryId methods allow us to find and assign an Id for each model. 

### Populate MetaData
The next step was to edit the Interface.MetaData file.

#### Scopes
There are no scopes for this integration, so we can skip this one.

#### Presets
This integration does not require authentication, so our only preset is API Url

#### GetTables
Based on what we are trying to integrate, we have three primary models that we need to create tables from: Users (Customers in iPaaS), Products, and Carts (Transactions in iPaaS). We also have two child models that we will want to map to, UserAddress (CustomerAddress in iPaaS) and CardProdyuct (TransactionLine in iPaaS).

### Edit Settings
Next we editted the Itnerface.Settings file. Since we only have an API Url, we removed the other settings.

### Edit TranslationUtilities
Now we edit Interface.TranslationUtilities. Since we followed the template and implemented AbstractIntegrationData, much of the work here is done for us. But there are a few methods we need to add some code to.

#### GetDestinationObject
We need to add a case and return value for each primary model we created. This method is called by iPaaS to create a model instance as needed. It is basically a way for us to connect the models we've created with the existing iPaaS Mapping Collection Types. 

We only have three models, which we add to the switch statement.

#### GetPrimaryId
For my implementation, I chose to create two child models that do not implement AbstractIntegrationData. Because of that, I need to provide a way for GetPrimaryId to get ids for these models. So I add some code for that.

Note that neither of these models had clear Id candidate fields, so we had to add some code to handle this. See the comment at line 105 of User.cs or at line 99 of Cart.cs for more details on how we handled this.

Since we are only going to be pulling data from FakeStore, we only need to modify GetPrimaryId. If we were also going to send data to FakeStore, we would need to make similar changes to SetPrimaryId.


#### GetChildMappings
We have three primary models and two child models. So I modified the GetChildMappings method to return the child objects for each mapping collection type. This method is where we connect a mapping collection type to its parent and the field in the model on that parent.

I added the Cart.Products field as a Transaction Line type and the User.Address field as a Customer Address type.

### Create DevTests
We created a series of simple tests to confirm that we can preform all the necessary CRUD operations on our models. Later we will execute these.

### Change the Namespace
Unless you've changed it, the template namespace does not match the namespace that we will be sending to iPaaS. This value is import to allow iPaaS to correctly find the types required to run.

We changed the namespace from Integration.Data.Interface to FakeStore.Data.Interface in every file.

### Create an Initial System
In iPaaS, I created a system for my FakeStore integration. Since we haven't uploaded our file yet, there will not be any settings to fill in yet. We just need this system as a place holder.

### Upload the File
In the Integration Development Utility, I modified the app settings to point to my FakeStore.Data.dll. Once that is done, I executed the program and ran the UPLOAD command.

### Create a New System
Now that the file is uploaded and the metadata added, I created a new system. I confirmed that my one setting (API Url) was present. I set this to the FakeStore URL: https://fakestoreapi.com/

Since I didn't need it anymore, I also deleted the initial system I had created.

### Execute DevelopmentTests

Now that I have a valid system, I could run my DevelopmentTests. In my case, my system was created with ID 10684, so I could run these commands and confirm that they executed without errros:

> TEST Product_Get 10684
> TEST Product_Update 10684
> TEST Product_Create 10684
> TEST Product_Delete 10684

> TEST User_Get 10684
> TEST User_Update 10684
> TEST User_Create 10684
> TEST User_Delete 10684

> TEST Cart_Get 10684
> TEST Cart_Update 10684
> TEST Cart_Create 10684
> TEST Cart_Delete 10684

In my case, I did find a few errors. So I corrected them and uploaded a fresh file.

### Create Mappings in iPaaS

Now that all of the initial code is complete, I can start defining mappings in iPaaS. 

#### Customers
I started with customers/users. The first field I mapped is CustomerNumber. There isn't a field that corresponds exactly to this in FakeStore, but there is an Id, so I decided to appended FS- to the Id with a dynamic formula.

For the first and last names, FakeStore has these in a data structure called Name, so I need to use a formula to access them: Name.FirstName and Name.LastName, respectively.

For Email, we can do a simple field mapping.

I also defined a mapping for the customer's address. Since the address does not have a first or last name in FakeStore, I used a formula to access the data from the Parent's .Name field. The Address1 field needed a formula to compine the Number and Street fields. The City and PostalCode fields were simple field mappings.

#### Products
Products in FakeStore are very simple and do not include secondary data like inventory or barcodes. I mapped the Sku, Description, DefaultPrice, and Name to the corresponding fields in FakeStore. 

Products in iPaaS require Type, Status, and TrackingNumber. Since FakeStore doesn't have corresponding fields for these, I set static mappings of Physical, Active, and Product, respectively.

#### Transactions
For transactions, we must specify a SystemId. The easiest way to get this is with the universal variable, SpaceportSystemId. This variable will always tell us what the current system's id is.

For the TransactionNumber, I decided to append FSO- to the Id to make a more readable transaction number. For CustomerId, we need to specify the iPaaS customer id, but all we have is the FakeStore UserId. So we have to use a formula to translate that. The GetSpaceportId allows us to do that. We specify the source id, the type, and the system id, and it will return the corresponding iPaaS id.

Type and Status are required fields, so we set those as static mappings to Order and Pending, respectively.

For Total, the FakeStore Cart endpoint does not have this field. But it does include information that allows us to calculate it. This is a complicated effort, so I chose to define a method in the ConversionFunctions file. I created two methods: one that would calculate the price for a single item (CalculateCartProductTotalAsync), and one that would calculate the price for all items (CalculateCartTotalAsync). Once I defined those functions, I built and uploaded a new dll.

Finally, I defined a mapping for Transaction Lines. The Type and Status fields are required, so I set static mappings with the values Product and Pending. I mapped the ProductId to the Sku and the Qty field to the Quantity. The ExtendedPrice field needed a price, so I used the formula I defined to help with the Transaction Total for this.

### Testing Hooks
Now that I had my file complete and my mappings in place, I decided to test some data transfers. First I ran some products, using this command in the Development Utility:

> HOOK 10684 product/created 1 TO
> HOOK 10684 product/created 2 TO
> HOOK 10684 product/created 3 TO

This told iPaaS that I wanted to send products 1, 2, and 3 in system 10684 TO iPaaS with the scope product/created. I reviewed the items in iPaaS and confirmed that the data I expected there was present.

Then I sent hooks to test customers and transactions:

> HOOK 10684 customer/created 1 TO
> HOOK 10684 transaction/created 1 TO

Once I confirmed that those hooks worked correctly, I was done.

