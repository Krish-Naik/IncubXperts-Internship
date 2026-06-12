const msal = require('@azure/msal-node');
const env = require('./env');

module.exports = new msal.ConfidentialClientApplication({
    auth: {
        clientId: env.clientId,
        authority: `https://login.microsoftonline.com/${env.tenantId}`,
        clientSecret: env.clientSecret,
    },
    system: {
        loggerOptions: {
            loggerCallback() {},
            piiLoggingEnabled: false,
            logLevel: msal.LogLevel.Error,
        },
    },
});
