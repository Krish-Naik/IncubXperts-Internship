targetScope = 'resourceGroup'

@description('Azure region - choose one close to you')
param location string = resourceGroup().location

@description('SQL admin username')
param sqlAdminLogin string = 'losadmin'

@secure()
@description('SQL admin password - min 8 chars, upper, lower, number, special')
param sqlAdminPassword string

@description('JWT signing key for the API')
@secure()
param jwtSigningKey string

@description('Frontend URL for CORS')
param frontendUrl string = 'https://placeholder.azurestaticapps.net'

var uniqueSuffix = uniqueString(resourceGroup().id)
var sqlServerName = 'los-sql-${uniqueSuffix}'
var apiAppName = 'los-api-${uniqueSuffix}'
var appPlanName = 'los-plan-${uniqueSuffix}'
var staticWebAppName = 'los-web-${uniqueSuffix}'

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'losdb'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
  }
}

resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appPlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: false
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'Jwt__Issuer'
          value: 'LOS.Api'
        }
        {
          name: 'Jwt__Audience'
          value: 'LOS.Frontend'
        }
        {
          name: 'Jwt__SigningKey'
          value: jwtSigningKey
        }
        {
          name: 'Cors__AllowedOrigins__0'
          value: frontendUrl
        }
        {
          name: 'Frontend__BaseUrl'
          value: frontendUrl
        }
        {
          name: 'Cors__AllowedOrigins__1'
          value: 'http://localhost:4200'
        }
      ]
    }
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: 'eastus2'
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'