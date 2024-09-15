# Payments Virtual Table Provider

The PaymentVirtualTableProvider is a plugin that provides a virtual table for handling payments in the Contoso Real Estate Power App. It allows users to manage and process payments within the app and for access via the portal.

## Notes

1. The QueryExpression sent from Dataverse is converted into a OData query and sent to the Azure Functions API
1. When building the solution project, there are extra build tasks to publish and pack so that `dotnet build` will create a `nupkg` file
1. The solution packager `solution-mapping.xml` contains a reference to the `nupkg` because `pac solution add-reference` does not work with nuget package plugins at this time.
1. For more information on error returns and exceptions related to the PaymentVirtualTableProvider, refer to the [Data Provider Exceptions](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/virtual-entities/custom-ve-data-providers#data-provider-exceptions) documentation.
