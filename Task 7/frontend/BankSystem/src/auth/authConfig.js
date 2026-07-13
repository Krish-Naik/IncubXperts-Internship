const entraTenantId = import.meta.env.VITE_ENTRA_TENANT_ID;
const entraClientId = import.meta.env.VITE_ENTRA_CLIENT_ID;
const backendApiClientId = import.meta.env.VITE_BACKEND_API_CLIENT_ID;

export const msalConfig = {
  auth: {
    clientId: entraClientId,
    authority: `https://login.microsoftonline.com/${entraTenantId}`,
    redirectUri: "http://localhost:5173",
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
};

export const loginRequest = {
  scopes: [`api://${backendApiClientId}/access_as_user`],
};
