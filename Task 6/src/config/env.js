require('dotenv').config();

const required = [
    'CLIENT_ID',
    'TENANT_ID',
    'CLIENT_SECRET',
    'REDIRECT_URI',
    'EXPRESS_SESSION_SECRET',
];

for (const key of required) {
    if (!process.env[key]) {
        throw new Error(`Missing required environment variable: ${key}`);
    }
}

module.exports = {
    port: process.env.PORT || 3000,
    isProd: process.env.NODE_ENV === 'production',
    clientId: process.env.CLIENT_ID,
    tenantId: process.env.TENANT_ID,
    clientSecret: process.env.CLIENT_SECRET,
    redirectUri: process.env.REDIRECT_URI,
    postLogoutRedirectUri: process.env.POST_LOGOUT_REDIRECT_URI || 'http://localhost:3000',
    expressSessionSecret: process.env.EXPRESS_SESSION_SECRET,
};
