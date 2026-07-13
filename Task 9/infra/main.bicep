targetScope = 'resourceGroup'

@description('Azure region')
param location string = resourceGroup().location

@description('Neon PostgreSQL connection string')
@secure()
param postgresConnectionString string

@description('JWT signing key for the API')
@secure()
param jwtSigningKey string

@description('Frontend URL for CORS')
param frontendUrl string = 'https://placeholder.azurestaticapps.net'

@secure()
param sendGridApiKey string = ''

param sendGridFromEmail string = ''

var uniqueSuffix = uniqueString(resourceGroup().id)
var apiAppName = 'los-api-${uniqueSuffix}'
var appPlanName = 'los-plan-${uniqueSuffix}'
var staticWebAppName = 'los-web-${uniqueSuffix}'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appPlanName
  location: location
  sku: { name: 'F1', tier: 'Free', size: 'F1', capacity: 1 }
  kind: 'linux'
  properties: { reserved: true }
}

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: false
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'ConnectionStrings__DefaultConnection', value: postgresConnectionString }
        { name: 'Jwt__Issuer', value: 'LOS.Api' }
        { name: 'Jwt__Audience', value: 'LOS.Frontend' }
        { name: 'Jwt__SigningKey', value: jwtSigningKey }
        { name: 'Cors__AllowedOrigins__0', value: frontendUrl }
        { name: 'Frontend__BaseUrl', value: frontendUrl }
        { name: 'Email__Provider', value: 'SendGrid' }
        { name: 'Email__SendGrid__ApiKey', value: sendGridApiKey }
        { name: 'Email__SendGrid__FromEmail', value: sendGridFromEmail }
        { name: 'Email__SendGrid__FromName', value: 'LOS Notifications' }
        { name: 'Storage__Provider', value: 'LocalDisk' }
        { name: 'Storage__LocalDiskBasePath', value: 'App_Data/uploads' }
        { name: 'Seed__RunOnStartup', value: 'false' }
      ]
    }
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: 'eastus2'
  sku: { name: 'Free', tier: 'Free' }
  properties: {}
}

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'