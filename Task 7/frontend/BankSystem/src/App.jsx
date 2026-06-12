import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAuth } from "./auth/authContext";
import {
  closeAccount,
  createAccount,
  depositOwn,
  depositToAccount,
  getAccountTransactions,
  getAllAccounts,
  getCurrentUserProfile,
  getOwnAccounts,
  getOwnTransactions,
  searchCustomers,
  transferBetweenAccounts,
  updateAccount,
  withdrawFromAccount,
  withdrawOwn,
} from "./api/bankingApi";

// ─── Initial form states ──────────────────────────────────────────────────────

const initialCreateForm = {
  customerId: "",
  customerDisplayName: "",
  type: "SAVINGS",
  openingBalance: "",
};

const initialUpdateForm = {
  id: "",
  holderName: "",
  email: "",
  type: "SAVINGS",
  isActive: true,
};

const initialAmountForm = {
  accountId: "",
  amount: "",
};

const initialTransferForm = {
  senderId: "",
  receiverId: "",
  amount: "",
};

// ─── Shared UI primitives ────────────────────────────────────────────────────

function Section({ title, children }) {
  return (
    <section
      style={{
        border: "1px solid #e2e8f0",
        borderRadius: 12,
        padding: 20,
        marginBottom: 20,
        background: "#ffffff",
      }}
    >
      <h2 style={{ marginTop: 0, marginBottom: 16 }}>{title}</h2>
      {children}
    </section>
  );
}

function Field({ label, children }) {
  return (
    <label style={{ display: "block", marginBottom: 14 }}>
      <div style={{ fontWeight: 600, marginBottom: 6 }}>{label}</div>
      {children}
    </label>
  );
}

function Input(props) {
  return (
    <input
      {...props}
      style={{
        width: "100%",
        padding: "10px 12px",
        border: "1px solid #cbd5e1",
        borderRadius: 10,
        outline: "none",
        boxSizing: "border-box",
        ...props.style,
      }}
    />
  );
}

function Select(props) {
  return (
    <select
      {...props}
      style={{
        width: "100%",
        padding: "10px 12px",
        border: "1px solid #cbd5e1",
        borderRadius: 10,
        outline: "none",
        boxSizing: "border-box",
      }}
    />
  );
}

function Button({ children, variant = "primary", style, ...props }) {
  const variants = {
    primary: {
      background: "#0f172a",
      color: "#fff",
      border: "1px solid #0f172a",
    },
    secondary: {
      background: "#fff",
      color: "#0f172a",
      border: "1px solid #cbd5e1",
    },
    danger: {
      background: "#b91c1c",
      color: "#fff",
      border: "1px solid #b91c1c",
    },
    success: {
      background: "#15803d",
      color: "#fff",
      border: "1px solid #15803d",
    },
  };

  return (
    <button
      {...props}
      style={{
        padding: "10px 14px",
        borderRadius: 10,
        cursor: props.disabled ? "not-allowed" : "pointer",
        marginRight: 8,
        marginBottom: 8,
        opacity: props.disabled ? 0.5 : 1,
        ...variants[variant],
        ...style,
      }}
    >
      {children}
    </button>
  );
}

/**
 * Generic list for both BankAccountResponse and CustomerLookupResponse.
 * Distinguishes by checking for `accountNumber` in the item.
 */
function ResultList({
  items,
  onSelect,
  selectedId,
  emptyText = "No results found.",
}) {
  if (items.length === 0) {
    return (
      <div
        style={{
          padding: 12,
          color: "#64748b",
          border: "1px solid #e2e8f0",
          borderRadius: 10,
        }}
      >
        {emptyText}
      </div>
    );
  }

  return (
    <div
      style={{
        maxHeight: 280,
        overflowY: "auto",
        border: "1px solid #e2e8f0",
        borderRadius: 10,
      }}
    >
      {items.map((item) => {
        const isSelected = item.id === selectedId;
        const isAccount = "accountNumber" in item;

        return (
          <button
            key={item.id}
            type="button"
            onClick={() => onSelect(item)}
            style={{
              width: "100%",
              textAlign: "left",
              padding: 12,
              border: "none",
              borderBottom: "1px solid #e2e8f0",
              background: isSelected ? "#eff6ff" : "#fff",
              cursor: "pointer",
            }}
          >
            <div style={{ fontWeight: 600 }}>
              {item.holderName ?? item.displayName ?? "Unknown"}
            </div>
            <div style={{ fontSize: 14, color: "#475569" }}>{item.email}</div>
            {isAccount && (
              <div style={{ fontSize: 13, color: "#64748b", marginTop: 4 }}>
                {item.accountNumber} · {item.type} · Balance: {item.balance}
                {!item.isActive && (
                  <span
                    style={{
                      marginLeft: 8,
                      color: "#b91c1c",
                      fontWeight: 600,
                    }}
                  >
                    CLOSED
                  </span>
                )}
              </div>
            )}
          </button>
        );
      })}
    </div>
  );
}

// ─── Main app ────────────────────────────────────────────────────────────────

export default function App() {
  const {
    user,
    signIn,
    signOut,
    restoreSession,
    isAuthenticated,
    isLoading,
    authError,
  } = useAuth();

  const didRestore = useRef(false);

  // ── Core state ──────────────────────────────────────────────────────────────
  const [me, setMe] = useState(null);
  const [accounts, setAccounts] = useState([]);
  const [transactions, setTransactions] = useState([]);
  const [searchText, setSearchText] = useState("");
  const [selectedAccountId, setSelectedAccountId] = useState("");
  const [apiResult, setApiResult] = useState("");
  const [isBusy, setIsBusy] = useState(false);

  // ── Form state ──────────────────────────────────────────────────────────────
  const [createForm, setCreateForm] = useState(initialCreateForm);
  const [updateForm, setUpdateForm] = useState(initialUpdateForm);
  const [depositForm, setDepositForm] = useState(initialAmountForm);
  const [withdrawForm, setWithdrawForm] = useState(initialAmountForm);
  const [transferForm, setTransferForm] = useState(initialTransferForm);

  // ── Customer search state (for Create Account) ──────────────────────────────
  const [customerSearch, setCustomerSearch] = useState("");
  const [customerResults, setCustomerResults] = useState([]);
  const [isSearchingCustomers, setIsSearchingCustomers] = useState(false);

  // ── Receiver search state (for Transfer) ───────────────────────────────────
  const [receiverSearch, setReceiverSearch] = useState("");
  const [receiverResults, setReceiverResults] = useState([]);
  const [isSearchingReceiver, setIsSearchingReceiver] = useState(false);

  // ── Session restore on mount ────────────────────────────────────────────────
  useEffect(() => {
    if (didRestore.current) return;
    didRestore.current = true;
    restoreSession();
  }, [restoreSession]);

  // ── Permission flags ────────────────────────────────────────────────────────
  const permissions = user?.permissions ?? [];

  const canReadOwnAccounts = permissions.includes("accounts.own.read");
  const canDepositOwn = permissions.includes("accounts.own.deposit");
  const canWithdrawOwn = permissions.includes("accounts.own.withdraw");
  const canReadOwnTransactions = permissions.includes("transactions.own.read");

  const canReadAllAccounts = permissions.includes("accounts.all.read");
  const canCashDeposit = permissions.includes("accounts.cash.deposit");
  const canCashWithdraw = permissions.includes("accounts.cash.withdraw");
  const canTransfer = permissions.includes("transactions.transfer");
  const canCreateAccount = permissions.includes("accounts.create");
  const canUpdateAccount = permissions.includes("accounts.update");
  const canCloseAccount = permissions.includes("accounts.close");
  const canReadAllTransactions = permissions.includes("transactions.all.read");

  /**
   * Staff means "has access to all accounts" (Teller, Manager, Admin).
   * Used to route deposit/withdraw to the correct endpoint.
   */
  const isStaff = canReadAllAccounts;

  const selectedAccount = useMemo(
    () => accounts.find((a) => a.id === selectedAccountId) ?? null,
    [accounts, selectedAccountId],
  );

  // ── Result helpers ──────────────────────────────────────────────────────────
  const setResult = (label, data) => {
    setApiResult(`${label}\n\n${JSON.stringify(data, null, 2)}`);
  };

  const setError = (label, error) => {
    setResult(label, {
      status: error?.response?.status,
      data: error?.response?.data,
      message: error?.message,
    });
  };

  const runAction = useCallback(async (label, action) => {
    setIsBusy(true);
    try {
      const result = await action();
      if (label) setResult(label, result);
      return result;
    } catch (error) {
      setError(`${label} failed`, error);
      throw error;
    } finally {
      setIsBusy(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── API actions ─────────────────────────────────────────────────────────────

  const loadProfile = async () => {
    const data = await runAction("GET /api/auth/me", () =>
      getCurrentUserProfile(),
    );
    setMe(data);
  };

  const loadAccounts = async () => {
    if (isStaff) {
      const data = await runAction("GET /api/accounts", () =>
        getAllAccounts(searchText),
      );
      setAccounts(data);
      setTransactions([]);
      if (data.length > 0) setSelectedAccountId(data[0].id);
    } else {
      const data = await runAction("GET /api/accounts/my", () =>
        getOwnAccounts(),
      );
      setAccounts(data);
      setTransactions([]);
      if (data.length > 0) setSelectedAccountId(data[0].id);
    }
  };

  const loadTransactions = async () => {
    if (!selectedAccountId) return;

    const data = canReadAllTransactions
      ? await runAction(
          `GET /api/accounts/${selectedAccountId}/transactions`,
          () => getAccountTransactions(selectedAccountId),
        )
      : await runAction(
          `GET /api/accounts/my/${selectedAccountId}/transactions`,
          () => getOwnTransactions(selectedAccountId),
        );

    setTransactions(data);
  };

  /**
   * When a row in the accounts list is clicked:
   * pre-fills Update, Deposit, Withdraw, and Transfer-sender forms.
   * Create Account has its own dedicated customer-search picker.
   */
  const selectAccount = (account) => {
    setSelectedAccountId(account.id);

    setUpdateForm({
      id: account.id,
      holderName: account.holderName,
      email: account.email,
      type: account.type,
      isActive: account.isActive,
    });

    setDepositForm({ accountId: account.id, amount: "" });
    setWithdrawForm({ accountId: account.id, amount: "" });
    setTransferForm((prev) => ({ ...prev, senderId: account.id }));
  };

  // ── Customer search (for Create Account) ────────────────────────────────────

  const handleSearchCustomers = async () => {
    setIsSearchingCustomers(true);
    setCustomerResults([]);
    try {
      const data = await searchCustomers(customerSearch);
      setCustomerResults(data ?? []);
    } catch (error) {
      setError("GET /api/users/customers failed", error);
    } finally {
      setIsSearchingCustomers(false);
    }
  };

  const selectCustomerForCreate = (customer) => {
    setCreateForm((prev) => ({
      ...prev,
      customerId: customer.id,
      customerDisplayName: customer.displayName,
    }));
    setCustomerResults([]);
    setCustomerSearch("");
  };

  const clearSelectedCustomer = () => {
    setCreateForm((prev) => ({
      ...prev,
      customerId: "",
      customerDisplayName: "",
    }));
  };

  // ── Receiver search (for Transfer) ──────────────────────────────────────────

  const handleSearchReceiver = async () => {
    setIsSearchingReceiver(true);
    setReceiverResults([]);
    try {
      const data = await getAllAccounts(receiverSearch);
      setReceiverResults(
        (data ?? []).filter((a) => a.id !== transferForm.senderId),
      );
    } catch (error) {
      setError("Search receiver accounts failed", error);
    } finally {
      setIsSearchingReceiver(false);
    }
  };

  const selectReceiverAccount = (account) => {
    setTransferForm((prev) => ({ ...prev, receiverId: account.id }));
    setReceiverResults([]);
    setReceiverSearch("");
  };

  // ── Form submit handlers ────────────────────────────────────────────────────

  const handleCreateAccount = async (e) => {
    e.preventDefault();

    if (!createForm.customerId) {
      setApiResult(
        "Validation error: Please search for and select a customer before submitting.",
      );
      return;
    }

    // CreateAccountRequest: { AppUserId, Type, OpeningBalance }
    const payload = {
      appUserId: createForm.customerId,
      type: createForm.type,
      openingBalance: Number(createForm.openingBalance || 0),
    };

    try {
      const data = await runAction("POST /api/accounts", () =>
        createAccount(payload),
      );
      setCreateForm(initialCreateForm);
      setCustomerSearch("");
      setCustomerResults([]);
      await loadAccounts();
      return data;
    } catch {
      // Error already surfaced via runAction → setError
    }
  };

  const handleUpdateAccount = async (e) => {
    e.preventDefault();

    const payload = {
      holderName: updateForm.holderName.trim(),
      email: updateForm.email.trim(),
      type: updateForm.type,
      isActive: updateForm.isActive,
    };

    try {
      await runAction(`PUT /api/accounts/${updateForm.id}`, () =>
        updateAccount(updateForm.id, payload),
      );
      await loadAccounts();
    } catch {
      // Error already surfaced via runAction → setError
    }
  };

  const handleCloseAccount = async () => {
    if (!selectedAccountId) return;
    try {
      await runAction(`PUT /api/accounts/${selectedAccountId}/close`, () =>
        closeAccount(selectedAccountId),
      );
      await loadAccounts();
    } catch {
      // Error already surfaced via runAction → setError
    }
  };

  const handleDeposit = async (e) => {
    e.preventDefault();
    try {
      if (isStaff) {
        await runAction(
          `PUT /api/accounts/${depositForm.accountId}/deposit`,
          () =>
            depositToAccount(depositForm.accountId, Number(depositForm.amount)),
        );
      } else {
        await runAction(
          `PUT /api/accounts/my/${depositForm.accountId}/deposit`,
          () => depositOwn(depositForm.accountId, Number(depositForm.amount)),
        );
      }
      await loadAccounts();
    } catch {
      // Error already surfaced via runAction → setError
    }
  };

  const handleWithdraw = async (e) => {
    e.preventDefault();
    try {
      if (isStaff) {
        await runAction(
          `PUT /api/accounts/${withdrawForm.accountId}/withdraw`,
          () =>
            withdrawFromAccount(
              withdrawForm.accountId,
              Number(withdrawForm.amount),
            ),
        );
      } else {
        await runAction(
          `PUT /api/accounts/my/${withdrawForm.accountId}/withdraw`,
          () =>
            withdrawOwn(withdrawForm.accountId, Number(withdrawForm.amount)),
        );
      }
      await loadAccounts();
    } catch {
      // Error already surfaced via runAction → setError
    }
  };

  const handleTransfer = async (e) => {
    e.preventDefault();
    try {
      await runAction(
        `PUT /api/accounts/${transferForm.senderId}/transfer`,
        () =>
          transferBetweenAccounts(
            transferForm.senderId,
            transferForm.receiverId,
            Number(transferForm.amount),
          ),
      );
      await loadAccounts();
    } catch {
      // Error already surfaced via runAction → setError
    }
  };

  // ── Render ──────────────────────────────────────────────────────────────────

  return (
    <div
      style={{
        maxWidth: 1280,
        margin: "0 auto",
        padding: 24,
        background: "#f8fafc",
        minHeight: "100vh",
        fontFamily: "Arial, sans-serif",
      }}
    >
      <h1 style={{ marginBottom: 8 }}>Bank System Dashboard</h1>

      {isLoading && <p style={{ color: "#475569" }}>Signing in…</p>}
      {authError && <p style={{ color: "#b91c1c" }}>Auth error: {authError}</p>}

      {!isAuthenticated ? (
        <Button onClick={signIn} disabled={isLoading}>
          Login with Entra ID
        </Button>
      ) : (
        <>
          {/* ── Session card ─────────────────────────────────────────── */}
          <Section title="Session">
            <p>
              <strong>Name:</strong> {user.displayName}
            </p>
            <p>
              <strong>Email:</strong> {user.email}
            </p>
            <p>
              <strong>Role:</strong>{" "}
              <span
                style={{
                  background: "#0f172a",
                  color: "#fff",
                  padding: "2px 10px",
                  borderRadius: 8,
                  fontSize: 13,
                }}
              >
                {user.role}
              </span>
            </p>
            <p style={{ wordBreak: "break-all" }}>
              <strong>Permissions:</strong> {permissions.join(", ")}
            </p>

            <div style={{ marginTop: 14 }}>
              <Button onClick={loadProfile} disabled={isBusy}>
                Load My Profile
              </Button>
              <Button onClick={loadAccounts} disabled={isBusy}>
                {isStaff ? "Search / Load Accounts" : "Load My Accounts"}
              </Button>
              <Button variant="danger" onClick={signOut}>
                Logout
              </Button>
            </div>
          </Section>

          {/* ── Current user profile ─────────────────────────────────── */}
          {me && (
            <Section title="Current User (from /api/auth/me)">
              <pre style={{ whiteSpace: "pre-wrap", margin: 0, fontSize: 13 }}>
                {JSON.stringify(me, null, 2)}
              </pre>
            </Section>
          )}

          {/* ── Accounts list / search ────────────────────────────────── */}
          {(canReadOwnAccounts || canReadAllAccounts) && (
            <Section
              title={isStaff ? "Account Search & Selection" : "My Accounts"}
            >
              {isStaff && (
                <div
                  style={{
                    display: "flex",
                    gap: 8,
                    marginBottom: 12,
                    alignItems: "flex-end",
                  }}
                >
                  <div style={{ flex: 1 }}>
                    <Field label="Search by customer name, email, or account number">
                      <Input
                        value={searchText}
                        onChange={(e) => setSearchText(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && loadAccounts()}
                        placeholder="Search accounts…"
                      />
                    </Field>
                  </div>
                  <Button
                    onClick={loadAccounts}
                    disabled={isBusy}
                    style={{ marginBottom: 14 }}
                  >
                    Search
                  </Button>
                </div>
              )}

              {!isStaff && (
                <Button
                  onClick={loadAccounts}
                  disabled={isBusy}
                  style={{ marginBottom: 12 }}
                >
                  Refresh
                </Button>
              )}

              {accounts.length > 0 ? (
                <ResultList
                  items={accounts}
                  selectedId={selectedAccountId}
                  onSelect={selectAccount}
                  emptyText="No accounts found."
                />
              ) : (
                <div style={{ color: "#64748b", fontSize: 14 }}>
                  No accounts loaded yet.{" "}
                  {isStaff ? "Use the search above." : 'Click "Refresh".'}
                </div>
              )}

              {selectedAccount && (
                <div
                  style={{
                    marginTop: 16,
                    padding: 14,
                    background: "#f0f9ff",
                    borderRadius: 10,
                    border: "1px solid #bae6fd",
                  }}
                >
                  <strong>Selected account:</strong>
                  <pre
                    style={{
                      whiteSpace: "pre-wrap",
                      margin: "8px 0 0",
                      fontSize: 13,
                    }}
                  >
                    {JSON.stringify(selectedAccount, null, 2)}
                  </pre>
                </div>
              )}
            </Section>
          )}

          {/* ── Create Account (Manager / Admin only) ────────────────── */}
          {canCreateAccount && (
            <Section title="Create Account">
              <p style={{ marginTop: 0, color: "#475569", fontSize: 14 }}>
                Search for an existing customer (registered user with the
                Customer role), select them, choose an account type and opening
                balance, then submit.
              </p>

              {/* Step 1: customer search */}
              <div
                style={{
                  marginBottom: 16,
                  padding: 16,
                  background: "#f8fafc",
                  borderRadius: 10,
                  border: "1px solid #e2e8f0",
                }}
              >
                <strong style={{ display: "block", marginBottom: 10 }}>
                  Step 1 — Find customer
                </strong>

                <div
                  style={{ display: "flex", gap: 8, alignItems: "flex-end" }}
                >
                  <div style={{ flex: 1 }}>
                    <Field label="Search customer by name or email">
                      <Input
                        value={customerSearch}
                        onChange={(e) => setCustomerSearch(e.target.value)}
                        onKeyDown={(e) =>
                          e.key === "Enter" && handleSearchCustomers()
                        }
                        placeholder="e.g. John, john@example.com"
                      />
                    </Field>
                  </div>
                  <Button
                    type="button"
                    onClick={handleSearchCustomers}
                    disabled={isSearchingCustomers}
                    style={{ marginBottom: 14 }}
                  >
                    {isSearchingCustomers ? "Searching…" : "Search"}
                  </Button>
                </div>

                {customerResults.length > 0 && (
                  <ResultList
                    items={customerResults}
                    selectedId={createForm.customerId}
                    onSelect={selectCustomerForCreate}
                    emptyText="No customers found."
                  />
                )}

                {createForm.customerId ? (
                  <div
                    style={{
                      marginTop: 10,
                      padding: "10px 14px",
                      background: "#f0fdf4",
                      border: "1px solid #86efac",
                      borderRadius: 8,
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                    }}
                  >
                    <span style={{ fontSize: 14 }}>
                      <strong>Selected:</strong>{" "}
                      {createForm.customerDisplayName}
                      <span
                        style={{
                          color: "#64748b",
                          marginLeft: 8,
                          fontSize: 12,
                        }}
                      >
                        ({createForm.customerId})
                      </span>
                    </span>
                    <button
                      type="button"
                      onClick={clearSelectedCustomer}
                      style={{
                        background: "none",
                        border: "none",
                        cursor: "pointer",
                        color: "#b91c1c",
                        fontSize: 13,
                        padding: "2px 6px",
                      }}
                    >
                      ✕ Clear
                    </button>
                  </div>
                ) : (
                  <div
                    style={{ marginTop: 10, color: "#94a3b8", fontSize: 13 }}
                  >
                    No customer selected yet.
                  </div>
                )}
              </div>

              {/* Step 2: account details */}
              <div
                style={{
                  padding: 16,
                  background: "#f8fafc",
                  borderRadius: 10,
                  border: "1px solid #e2e8f0",
                }}
              >
                <strong style={{ display: "block", marginBottom: 10 }}>
                  Step 2 — Account details
                </strong>

                <form onSubmit={handleCreateAccount}>
                  <Field label="Account Type">
                    <Select
                      value={createForm.type}
                      onChange={(e) =>
                        setCreateForm((prev) => ({
                          ...prev,
                          type: e.target.value,
                        }))
                      }
                    >
                      <option value="SAVINGS">SAVINGS</option>
                      <option value="CURRENT">CURRENT</option>
                    </Select>
                  </Field>

                  <Field label="Opening Balance">
                    <Input
                      type="number"
                      min="0"
                      step="0.01"
                      value={createForm.openingBalance}
                      onChange={(e) =>
                        setCreateForm((prev) => ({
                          ...prev,
                          openingBalance: e.target.value,
                        }))
                      }
                      placeholder="0.00"
                    />
                  </Field>

                  <Button
                    type="submit"
                    disabled={isBusy || !createForm.customerId}
                    variant={createForm.customerId ? "success" : "secondary"}
                  >
                    {isBusy ? "Creating…" : "Create Account"}
                  </Button>

                  {!createForm.customerId && (
                    <span
                      style={{ fontSize: 13, color: "#94a3b8", marginLeft: 4 }}
                    >
                      Select a customer in Step 1 first.
                    </span>
                  )}
                </form>
              </div>
            </Section>
          )}

          {/* ── Update / Close Account (Manager / Admin) ─────────────── */}
          {canUpdateAccount && (
            <Section title="Update or Close Account">
              <p style={{ marginTop: 0, color: "#475569", fontSize: 14 }}>
                Select an account from the list above to auto-fill the fields,
                or enter an account ID manually.
              </p>

              <form onSubmit={handleUpdateAccount}>
                <Field label="Account Id">
                  <Input
                    value={updateForm.id}
                    onChange={(e) =>
                      setUpdateForm((prev) => ({ ...prev, id: e.target.value }))
                    }
                    placeholder="Select from the list above…"
                  />
                </Field>

                <Field label="Holder Name">
                  <Input
                    value={updateForm.holderName}
                    onChange={(e) =>
                      setUpdateForm((prev) => ({
                        ...prev,
                        holderName: e.target.value,
                      }))
                    }
                  />
                </Field>

                <Field label="Email">
                  <Input
                    type="email"
                    value={updateForm.email}
                    onChange={(e) =>
                      setUpdateForm((prev) => ({
                        ...prev,
                        email: e.target.value,
                      }))
                    }
                  />
                </Field>

                <Field label="Account Type">
                  <Select
                    value={updateForm.type}
                    onChange={(e) =>
                      setUpdateForm((prev) => ({
                        ...prev,
                        type: e.target.value,
                      }))
                    }
                  >
                    <option value="SAVINGS">SAVINGS</option>
                    <option value="CURRENT">CURRENT</option>
                  </Select>
                </Field>

                <label
                  style={{
                    display: "flex",
                    alignItems: "center",
                    marginBottom: 16,
                    gap: 8,
                  }}
                >
                  <input
                    type="checkbox"
                    checked={updateForm.isActive}
                    onChange={(e) =>
                      setUpdateForm((prev) => ({
                        ...prev,
                        isActive: e.target.checked,
                      }))
                    }
                  />
                  <span style={{ fontWeight: 600 }}>Account Active</span>
                </label>

                <Button type="submit" disabled={isBusy || !updateForm.id}>
                  Update Account
                </Button>

                {canCloseAccount && (
                  <Button
                    type="button"
                    variant="danger"
                    onClick={handleCloseAccount}
                    disabled={isBusy || !selectedAccountId}
                  >
                    Close Selected Account
                  </Button>
                )}
              </form>
            </Section>
          )}

          {/* ── Deposit ──────────────────────────────────────────────── */}
          {(canDepositOwn || canCashDeposit) && (
            <Section
              title={isStaff ? "Deposit (Cash Desk)" : "Deposit to My Account"}
            >
              <p style={{ marginTop: 0, color: "#475569", fontSize: 14 }}>
                {isStaff
                  ? "Select any account from the search results above to auto-fill the Account Id."
                  : "Select one of your accounts from the list above to auto-fill the Account Id."}
              </p>

              <form onSubmit={handleDeposit}>
                <Field label="Account Id">
                  <Input
                    value={depositForm.accountId}
                    onChange={(e) =>
                      setDepositForm((prev) => ({
                        ...prev,
                        accountId: e.target.value,
                      }))
                    }
                    placeholder="Select from the list above…"
                  />
                </Field>

                <Field label="Amount">
                  <Input
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={depositForm.amount}
                    onChange={(e) =>
                      setDepositForm((prev) => ({
                        ...prev,
                        amount: e.target.value,
                      }))
                    }
                    placeholder="0.00"
                  />
                </Field>

                <Button
                  type="submit"
                  variant="success"
                  disabled={
                    isBusy || !depositForm.accountId || !depositForm.amount
                  }
                >
                  {isBusy ? "Processing…" : "Deposit"}
                </Button>
              </form>
            </Section>
          )}

          {/* ── Withdraw ─────────────────────────────────────────────── */}
          {(canWithdrawOwn || canCashWithdraw) && (
            <Section
              title={
                isStaff ? "Withdraw (Cash Desk)" : "Withdraw from My Account"
              }
            >
              <p style={{ marginTop: 0, color: "#475569", fontSize: 14 }}>
                {isStaff
                  ? "Select any account from the search results above to auto-fill the Account Id."
                  : "Select one of your accounts from the list above to auto-fill the Account Id."}
              </p>

              <form onSubmit={handleWithdraw}>
                <Field label="Account Id">
                  <Input
                    value={withdrawForm.accountId}
                    onChange={(e) =>
                      setWithdrawForm((prev) => ({
                        ...prev,
                        accountId: e.target.value,
                      }))
                    }
                    placeholder="Select from the list above…"
                  />
                </Field>

                <Field label="Amount">
                  <Input
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={withdrawForm.amount}
                    onChange={(e) =>
                      setWithdrawForm((prev) => ({
                        ...prev,
                        amount: e.target.value,
                      }))
                    }
                    placeholder="0.00"
                  />
                </Field>

                <Button
                  type="submit"
                  variant="danger"
                  disabled={
                    isBusy || !withdrawForm.accountId || !withdrawForm.amount
                  }
                >
                  {isBusy ? "Processing…" : "Withdraw"}
                </Button>
              </form>
            </Section>
          )}

          {/* ── Transfer (Teller / Manager / Admin) ──────────────────── */}
          {canTransfer && (
            <Section title="Transfer Between Accounts">
              <p style={{ marginTop: 0, color: "#475569", fontSize: 14 }}>
                Select the sender account from the accounts list above
                (auto-fills Sender Id), then search and select the receiver
                account below.
              </p>

              <form onSubmit={handleTransfer}>
                <Field label="Sender Account Id">
                  <Input
                    value={transferForm.senderId}
                    onChange={(e) =>
                      setTransferForm((prev) => ({
                        ...prev,
                        senderId: e.target.value,
                      }))
                    }
                    placeholder="Select from the list above…"
                  />
                </Field>

                {/* Receiver — inline account search */}
                <div
                  style={{
                    marginBottom: 14,
                    padding: 14,
                    background: "#f8fafc",
                    borderRadius: 10,
                    border: "1px solid #e2e8f0",
                  }}
                >
                  <div style={{ fontWeight: 600, marginBottom: 8 }}>
                    Receiver Account Id
                  </div>

                  <div style={{ display: "flex", gap: 8, marginBottom: 8 }}>
                    <div style={{ flex: 1 }}>
                      <Input
                        value={receiverSearch}
                        onChange={(e) => setReceiverSearch(e.target.value)}
                        onKeyDown={(e) =>
                          e.key === "Enter" && handleSearchReceiver()
                        }
                        placeholder="Search receiver by name, email, or account number…"
                        style={{ marginBottom: 0 }}
                      />
                    </div>
                    <Button
                      type="button"
                      onClick={handleSearchReceiver}
                      disabled={isSearchingReceiver}
                      variant="secondary"
                    >
                      {isSearchingReceiver ? "…" : "Search"}
                    </Button>
                  </div>

                  {receiverResults.length > 0 && (
                    <div style={{ marginBottom: 8 }}>
                      <ResultList
                        items={receiverResults}
                        selectedId={transferForm.receiverId}
                        onSelect={selectReceiverAccount}
                        emptyText="No accounts found."
                      />
                    </div>
                  )}

                  <Input
                    value={transferForm.receiverId}
                    onChange={(e) =>
                      setTransferForm((prev) => ({
                        ...prev,
                        receiverId: e.target.value,
                      }))
                    }
                    placeholder="Receiver account Id (auto-filled when you pick from search)"
                  />
                </div>

                <Field label="Amount">
                  <Input
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={transferForm.amount}
                    onChange={(e) =>
                      setTransferForm((prev) => ({
                        ...prev,
                        amount: e.target.value,
                      }))
                    }
                    placeholder="0.00"
                  />
                </Field>

                <Button
                  type="submit"
                  disabled={
                    isBusy ||
                    !transferForm.senderId ||
                    !transferForm.receiverId ||
                    !transferForm.amount
                  }
                >
                  {isBusy ? "Processing…" : "Transfer"}
                </Button>
              </form>
            </Section>
          )}

          {/* ── Transactions ─────────────────────────────────────────── */}
          {(canReadOwnTransactions || canReadAllTransactions) && (
            <Section title="Transactions">
              <p style={{ marginTop: 0, color: "#475569", fontSize: 14 }}>
                Select an account from the list above, then click the button to
                load its transactions.
              </p>

              <Button
                onClick={loadTransactions}
                disabled={isBusy || !selectedAccountId}
              >
                {selectedAccountId
                  ? "Load Transactions for Selected Account"
                  : "Select an account first"}
              </Button>

              {transactions.length > 0 && (
                <div style={{ marginTop: 16, overflowX: "auto" }}>
                  <table
                    style={{
                      width: "100%",
                      borderCollapse: "collapse",
                      fontSize: 13,
                    }}
                  >
                    <thead>
                      <tr style={{ background: "#f1f5f9" }}>
                        {[
                          "Type",
                          "Amount",
                          "Balance After",
                          "Description",
                          "By Role",
                          "Date",
                        ].map((h) => (
                          <th
                            key={h}
                            style={{
                              textAlign: "left",
                              padding: "8px 12px",
                              borderBottom: "1px solid #e2e8f0",
                              whiteSpace: "nowrap",
                            }}
                          >
                            {h}
                          </th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {transactions.map((tx) => (
                        <tr
                          key={tx.id}
                          style={{ borderBottom: "1px solid #f1f5f9" }}
                        >
                          <td style={{ padding: "8px 12px", fontWeight: 600 }}>
                            {tx.type}
                          </td>
                          <td style={{ padding: "8px 12px" }}>{tx.amount}</td>
                          <td style={{ padding: "8px 12px" }}>
                            {tx.balanceAfter}
                          </td>
                          <td style={{ padding: "8px 12px" }}>
                            {tx.description}
                          </td>
                          <td style={{ padding: "8px 12px" }}>
                            {tx.performedByRole}
                          </td>
                          <td
                            style={{
                              padding: "8px 12px",
                              whiteSpace: "nowrap",
                            }}
                          >
                            {new Date(tx.createdAtUtc).toLocaleString()}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {transactions.length === 0 && selectedAccountId && (
                <p style={{ color: "#94a3b8", fontSize: 13, marginTop: 12 }}>
                  No transactions loaded yet.
                </p>
              )}
            </Section>
          )}

          {/* ── API Output ───────────────────────────────────────────── */}
          <Section title="API Output">
            <pre
              style={{
                whiteSpace: "pre-wrap",
                margin: 0,
                fontSize: 13,
                color: apiResult.startsWith("Validation")
                  ? "#b91c1c"
                  : "inherit",
              }}
            >
              {apiResult || "No API call made yet."}
            </pre>
          </Section>
        </>
      )}
    </div>
  );
}
