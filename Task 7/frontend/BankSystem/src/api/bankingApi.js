import httpClient from "./httpClient";

function unwrap(response) {
  return response.data?.data ?? response.data?.Data ?? response.data;
}

export async function getCurrentUserProfile() {
  const response = await httpClient.get("/api/auth/me");
  return unwrap(response);
}

export async function searchCustomers(search = "") {
  const response = await httpClient.get("/api/users/customers", {
    params: { search },
  });
  return unwrap(response) ?? [];
}

export async function getOwnAccounts() {
  const response = await httpClient.get("/api/accounts/my");
  return unwrap(response) ?? [];
}

export async function getAllAccounts(search = "") {
  const response = await httpClient.get("/api/accounts", {
    params: { search },
  });
  return unwrap(response) ?? [];
}

export async function getOwnTransactions(accountId) {
  const response = await httpClient.get(
    `/api/accounts/my/${accountId}/transactions`,
  );
  return unwrap(response) ?? [];
}

export async function getAccountTransactions(accountId) {
  const response = await httpClient.get(
    `/api/accounts/${accountId}/transactions`,
  );
  return unwrap(response) ?? [];
}

export async function createAccount(payload) {
  const response = await httpClient.post("/api/accounts", payload);
  return response.data?.data ?? response.data?.Data ?? response.data;
}

export async function updateAccount(accountId, payload) {
  const response = await httpClient.put(`/api/accounts/${accountId}`, payload);
  return unwrap(response);
}

export async function closeAccount(accountId) {
  const response = await httpClient.put(`/api/accounts/${accountId}/close`);
  return unwrap(response);
}

export async function depositOwn(accountId, amount) {
  const response = await httpClient.put(
    `/api/accounts/my/${accountId}/deposit`,
    {
      amount,
    },
  );
  return unwrap(response);
}

export async function withdrawOwn(accountId, amount) {
  const response = await httpClient.put(
    `/api/accounts/my/${accountId}/withdraw`,
    {
      amount,
    },
  );
  return unwrap(response);
}

export async function depositToAccount(accountId, amount) {
  const response = await httpClient.put(`/api/accounts/${accountId}/deposit`, {
    amount,
  });
  return unwrap(response);
}

export async function withdrawFromAccount(accountId, amount) {
  const response = await httpClient.put(`/api/accounts/${accountId}/withdraw`, {
    amount,
  });
  return unwrap(response);
}

export async function transferBetweenAccounts(senderId, receiverId, amount) {
  const response = await httpClient.put(`/api/accounts/${senderId}/transfer`, {
    receiverId,
    amount,
  });
  return unwrap(response);
}
