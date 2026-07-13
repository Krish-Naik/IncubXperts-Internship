const ACCESS_TOKEN_KEY = "bank_app_access_token";
const ACCESS_TOKEN_EXPIRY_KEY = "bank_app_access_token_expiry";
const USER_KEY = "bank_app_user";

export const tokenStorage = {
  setSession({ accessToken, accessTokenExpiresAtUtc, user }) {
    sessionStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    sessionStorage.setItem(ACCESS_TOKEN_EXPIRY_KEY, accessTokenExpiresAtUtc);
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
  },

  setAccessToken(token, expiresAtUtc) {
    sessionStorage.setItem(ACCESS_TOKEN_KEY, token);
    sessionStorage.setItem(ACCESS_TOKEN_EXPIRY_KEY, expiresAtUtc);
  },

  getAccessToken() {
    return sessionStorage.getItem(ACCESS_TOKEN_KEY);
  },

  getAccessTokenExpiry() {
    return sessionStorage.getItem(ACCESS_TOKEN_EXPIRY_KEY);
  },

  getUser() {
    const raw = sessionStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  },

  clear() {
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(ACCESS_TOKEN_EXPIRY_KEY);
    sessionStorage.removeItem(USER_KEY);
  },
};
