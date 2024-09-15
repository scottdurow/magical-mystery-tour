# The Object ID of the managed identity
$oidForMI = 'c086dd8d-a88e-4814-9e75-084ff44a78a6'
# The Application ID of the API app
$appID = 'd90d7a63-de78-4ac4-8507-f829425b01de'
$apiApp = az ad sp list --filter "appId eq '$appID'" | ConvertFrom-Json
$appOID = $apiApp.id
## The CanQueryPayments role
$roleguid=$apiApp[0].appRoles[1].id 
$data = "{'principalId': '$managedIdentityObjectId', 'resourceId': '$appOID','appRoleId': '$roleguid'}"    
$graphUrl =  "https://graph.microsoft.com/v1.0/servicePrincipals/$oidForMI/appRoleAssignments"
az rest -m POST -u $graphUrl --headers "Content-Type=application/json" -b $data
