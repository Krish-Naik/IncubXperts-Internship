import axios from "axios";
import httpClient from "../api/httpClient";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

function unwrap(response) {
  return response.data?.data ?? response.data?.Data ?? response.data;
}

export async function exchangeEntraToken(entraAccessToken) {
  const response = await axios.post(
    `${apiBaseUrl}/api/auth/exchange`,
    {},
    {
      headers: {
        Authorization: `Bearer ${entraAccessToken}`,
      },
      withCredentials: true,
    },
  );

  return unwrap(response);
}

export async function refreshAppToken() {
  const response = await axios.post(
    `${apiBaseUrl}/api/auth/refresh`,
    {},
    {
      withCredentials: true,
    },
  );

  return unwrap(response);
}

export async function logoutApp() {
  await httpClient.post("/api/auth/logout", {});
}
