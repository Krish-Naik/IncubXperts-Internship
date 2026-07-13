import { useCallback, useEffect, useMemo, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { exchangeEntraToken, logoutApp, refreshAppToken } from "./authApi";
import { loginRequest } from "./authConfig";
import { tokenStorage } from "./tokenStorage";
import { AuthContext } from "./authContext";

function isTokenStillValid(expiresAtUtc) {
  if (!expiresAtUtc) return false;
  const expiresAt = new Date(expiresAtUtc).getTime();
  return expiresAt - Date.now() > 60_000;
}

export function AuthProvider({ children }) {
  const { instance, accounts } = useMsal();
  const [user, setUser] = useState(() => tokenStorage.getUser() ?? null);
  const [authError, setAuthError] = useState(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!instance.getActiveAccount() && accounts.length > 0) {
      instance.setActiveAccount(accounts[0]);
    }
  }, [accounts, instance]);

  const applySession = useCallback((session) => {
    tokenStorage.setSession({
      accessToken: session.accessToken,
      accessTokenExpiresAtUtc: session.accessTokenExpiresAtUtc,
      user: session.user,
    });
    setUser(session.user);
  }, []);

  const clearSession = useCallback(() => {
    tokenStorage.clear();
    setUser(null);
  }, []);

  const restoreSession = useCallback(async () => {
    if (isLoading) return;

    setIsLoading(true);
    setAuthError(null);

    try {
      const storedUser = tokenStorage.getUser();
      const storedToken = tokenStorage.getAccessToken();
      const storedExpiry = tokenStorage.getAccessTokenExpiry();

      if (storedUser && storedToken && isTokenStillValid(storedExpiry)) {
        setUser(storedUser);
        return;
      }

      if (storedToken || storedUser) {
        try {
          const refreshed = await refreshAppToken();
          applySession(refreshed);
          return;
        } catch {
          clearSession();
        }
      }

      const account = instance.getActiveAccount();
      if (!account) {
        clearSession();
        return;
      }

      const tokenResult = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      });

      const appTokens = await exchangeEntraToken(tokenResult.accessToken);
      applySession(appTokens);
    } catch (error) {
      clearSession();
      console.error("restoreSession failed:", error);
      setAuthError(
        error?.response?.data?.message ??
          error?.response?.data?.Message ??
          error?.message ??
          "Authentication failed.",
      );
    } finally {
      setIsLoading(false);
    }
  }, [applySession, clearSession, instance, isLoading]);

  const signIn = useCallback(async () => {
    setAuthError(null);
    await instance.loginRedirect(loginRequest);
  }, [instance]);

  const signOut = useCallback(async () => {
    try {
      await logoutApp();
    } catch (error) {
      console.error("Backend logout failed:", error);
    } finally {
      clearSession();
      await instance.logoutRedirect({
        postLogoutRedirectUri: "http://localhost:5173",
      });
    }
  }, [clearSession, instance]);

  const value = useMemo(
    () => ({
      user,
      authError,
      isLoading,
      signIn,
      signOut,
      restoreSession,
      isAuthenticated: !!user,
    }),
    [user, authError, isLoading, signIn, signOut, restoreSession],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
