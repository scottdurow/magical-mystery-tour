# Managed Identity support

This virtual entity provider uses a managed identity to connect to the Azure resources (e.g. API)

See https://learn.microsoft.com/en-in/power-platform/admin/set-up-managed-identity

## Self Signed Certificate for development and testing

In production, a certificate issued by a certificate authority should be used, however for development and testing, the `dev-certificate.pfx` in this folder can be used.

## Building

During the build, `SignTool.exe` is used to sign the certificate.

## Configuration

The Managed Identity support for Plugins allows either a Managed Identity or Application registraiton to be used to authenticate using a specific scope. In this sample, the Payments Virtual Entity Provider gets a token for the Payments API using the API Client Application using the scope stored in the environment variable `contoso_PaymentsApiResourceUrl`

1. A Managed Identity record is created in the solution, and the ApplicationId field set with the Managed Identity Object ID or the Application ID.
1. The Plugin Assembly record is updated to point to the Manage Identity
1. The Managed Identity or the Application Registration then have Federated Credentials Added:
    - Issuer URL: `https://[first 3 chars of environment id].[last 3 chars of the environment id].environment.api.powerplatform.com/sts`
        E.g. for an Environment ID: `750120dc-071f-ebd0-953d-d08ec4ca5508` of this would be `https://750120dc071febd0953dd08ec4ca55.08.environment.api.powerplatform.com/sts`
    - Subject identifier: component:pluginassembly,thumbprint:[Certificate Thumbprint],environment:[Environment ID]
        E.g. component:pluginassembly,thumbprint:5973A4C494859D34C8A0702ABEDE77FAFA70508A,environment:750120dc-071f-ebd0-953d-d08ec4ca5508

The Managed Identity and the Plugin Assembly can be included in the solution, however there must be post deployment updates:

1. The Application ID of the Managed Identity record must be updated to match the Contoso Payments API Client Application ID (or a Managed Identity Object ID).
1. The the Federated Credentials must be added to match the Environment ID
