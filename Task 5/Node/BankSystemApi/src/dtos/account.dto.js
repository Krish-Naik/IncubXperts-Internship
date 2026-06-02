function toAccountResponse(row) {
    return {
        id: row.id,
        accountNumber: row.account_number,
        holderName: row.holder_name,
        email: row.email,
        type: row.type,
        balance: Number(row.balance),
        isActive: row.is_active,
        createdAtUtc: row.created_at,
    };
}

module.exports = { toAccountResponse };
