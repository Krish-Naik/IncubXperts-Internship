class AccountRepository {
    constructor(db) {
        this.db = db;
    }

    async getAll() {
        const { rows } = await this.db.query(
            "SELECT * FROM bank_accounts ORDER BY created_at DESC",
        );
        return rows;
    }

    async getById(id) {
        const { rows } = await this.db.query(
            "SELECT * FROM bank_accounts WHERE id = $1",
            [id],
        );
        return rows[0] || null;
    }

    async getByEmail(email) {
        const { rows } = await this.db.query(
            "SELECT * FROM bank_accounts WHERE email = $1",
            [email],
        );
        return rows[0] || null;
    }

    async create(account) {
        const { rows } = await this.db.query(
            `INSERT INTO bank_accounts (account_number, holder_name, email, type, balance, is_active)
         VALUES ($1, $2, $3, $4, $5, $6)
         RETURNING *`,
            [
                account.accountNumber,
                account.holderName,
                account.email,
                account.type,
                account.balance,
                account.isActive,
            ],
        );

        return rows[0];
    }

    async update(id, data) {
        const { rows } = await this.db.query(
            `UPDATE bank_accounts
             SET holder_name = $1,
                 email = $2,
                 type = $3,
                 is_active = $4,
                 updated_at = NOW()
             WHERE id = $5
             RETURNING *`,
            [
                data.holderName,
                data.email,
                data.type,
                data.isActive,
                id,
            ],
        );
    
        return rows[0] || null;
    }

    async updateBalance(id, balance) {
        const { rows } = await this.db.query(
            `UPDATE bank_accounts
         SET balance = $2, updated_at = NOW()
         WHERE id = $1
         RETURNING *`,
            [id, balance],
        );

        return rows[0] || null;
    }

    async delete(id) {
        const { rows } = await this.db.query(
            "DELETE FROM bank_accounts WHERE id = $1 RETURNING *",
            [id],
        );

        return rows[0] || null;
    }
}

module.exports = AccountRepository;
