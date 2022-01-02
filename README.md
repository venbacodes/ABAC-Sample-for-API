# Attribute Based Access Control Sample for a .Net Core API Application

This is a simple demo sample on implementing ABAC in a .Net Core API Application. This uses three main aspects of Attributes => ACCESS, SCOPE and MODULE.

## Key Points
1. Access, Scope and Module attributes are used
2. It is possible to extend this sample to accomodate as many attributes as needed
3. No External libraries used
4. Handled all the necessary authn and authz in the handlers itself.
5. Optional takeaway - added an additional path for restricting resources in [PermissionsAuthHandler.cs#L60](https://github.com/venbacodes/ABAC-Sample-for-API/blob/main/Authorization/PermissionsAuthHandler.cs#L60)

## To Explore
1. Clone and run the code
2. Generate a JWT token with email/sub and exp. Applicable emails can be found in [TestUsers.cs](https://github.com/venbacodes/ABAC-Sample-for-API/blob/main/Model/TestUsers.cs)
3. Add the generated JWT token in the swagger auhtorization menu and call the APIs
