const crypto = require('crypto');
const msalClient = require('../config/msal');
const env = require('../config/env');

async function getAuthCodeUrl(session) {
    const state = crypto.randomBytes(16).toString('hex');
    session.authState = state;

    return msalClient.getAuthCodeUrl({
        scopes: ['openid', 'profile', 'email'],
        redirectUri: env.redirectUri,
        responseMode: 'query',
        state,
    });
}

async function getTokenByCode(code) {
    return msalClient.acquireTokenByCode({
        code,
        scopes: ['openid', 'profile', 'email'],
        redirectUri: env.redirectUri,
    });
}

function getLogoutUrl() {
    return `https://login.microsoftonline.com/${env.tenantId}/oauth2/v2.0/logout?post_logout_redirect_uri=${encodeURIComponent(env.postLogoutRedirectUri)}`;
}

module.exports = {
    getAuthCodeUrl,
    getTokenByCode,
    getLogoutUrl,
};
