import React from "react";
import ReactDOM from "react-dom/client";
import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import App from "./App";
import { msalConfig } from "./auth/authConfig";
import { AuthProvider } from "./auth/AuthProvider";

const msalInstance = new PublicClientApplication(msalConfig);

async function bootstrap() {
  await msalInstance.initialize();

  const redirectResult = await msalInstance.handleRedirectPromise();

  if (redirectResult?.account) {
    msalInstance.setActiveAccount(redirectResult.account);
  } else {
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0) {
      msalInstance.setActiveAccount(accounts[0]);
    }
  }

  ReactDOM.createRoot(document.getElementById("root")).render(
    <React.StrictMode>
      <MsalProvider instance={msalInstance}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MsalProvider>
    </React.StrictMode>,
  );
}

bootstrap().catch((error) => {
  console.error("MSAL bootstrap failed:", error);
  ReactDOM.createRoot(document.getElementById("root")).render(
    <div style={{ padding: "24px", fontFamily: "Arial, sans-serif" }}>
      <h1>Startup Error</h1>
      <pre style={{ color: "red", whiteSpace: "pre-wrap" }}>
        {String(error?.message || error)}
      </pre>
    </div>,
  );
});
