const AppError = require("../errors/AppError");
const { toAccountResponse } = require("../dtos/account.dto");
const {
    normalizeType,
    validateCreateAccount,
    validateAmount,
} = require("../validators/account.validator");

class AccountService {
    constructor(accountRepository) {
        this.accountRepository = accountRepository;
    }

    async getAll() {
        const rows = await this.accountRepository.getAll();
        return rows.map(toAccountResponse);
    }

    async getById(id) {
        const account = await this.accountRepository.getById(id);
        if (!account) throw new AppError("Account not found", 404);
        return toAccountResponse(account);
    }

    async create(body) {
        const errors = validateCreateAccount(body);
        if (errors.length) {
            throw new AppError(errors.join(", "), 400);
        }

        const normalizedEmail = body.email.trim().toLowerCase();
        const existing =
            await this.accountRepository.getByEmail(normalizedEmail);

        if (existing) {
            throw new AppError("Email already exists", 409);
        }

        const created = await this.accountRepository.create({
            accountNumber: this.generateAccountNumber(),
            holderName: body.holderName.trim(),
            email: normalizedEmail,
            type: normalizeType(body.type),
            balance: Number(body.openingBalance),
            isActive: true,
        });

        return toAccountResponse(created);
    }

    async update(id, body) {
        const current = await this.accountRepository.getById(id);
        if (!current) throw new AppError("Account not found", 404);
    
        if (!body || typeof body !== "object" || Array.isArray(body)) {
            throw new AppError("Request body is required", 400);
        }
    
        const holderName = typeof body.holderName === "string"
            ? body.holderName.trim()
            : "";
        const email = typeof body.email === "string"
            ? body.email.trim().toLowerCase()
            : "";
        const type = typeof body.type === "string"
            ? body.type.trim()
            : "";
    
        if (!holderName) {
            throw new AppError("holderName is required", 400);
        }
    
        if (!email) {
            throw new AppError("email is required", 400);
        }
    
        if (!type) {
            throw new AppError("type is required", 400);
        }
    
        if (typeof body.isActive !== "boolean") {
            throw new AppError("isActive must be true or false", 400);
        }
    
        const emailOwner = await this.accountRepository.getByEmail(email);
        if (emailOwner && emailOwner.id !== id) {
            throw new AppError("Email already exists", 409);
        }
    
        const updated = await this.accountRepository.update(id, {
            holderName,
            email,
            type: normalizeType(type),
            isActive: body.isActive,
        });
    
        return toAccountResponse(updated);
    }

    async withdraw(id, amount) {
        if (!validateAmount(amount)) {
            throw new AppError("Amount must be greater than zero", 400);
        }

        const account = await this.accountRepository.getById(id);
        if (!account) throw new AppError("Account not found", 404);
        if (!account.is_active)
            throw new AppError(
                "Inactive accounts cannot perform transactions",
                400,
            );
        if (Number(account.balance) < Number(amount)) {
            throw new AppError("Insufficient balance", 400);
        }

        const updated = await this.accountRepository.updateBalance(
            id,
            Number(account.balance) - Number(amount),
        );

        return toAccountResponse(updated);
    }

    async transfer(senderId, receiverId, amount) {
        if (!validateAmount(amount)) {
            throw new AppError("Amount must be greater than zero", 400);
        }

        if (senderId === receiverId) {
            throw new AppError(
                "Sender and receiver cannot be the same account",
                400,
            );
        }

        const sender = await this.accountRepository.getById(senderId);
        const receiver = await this.accountRepository.getById(receiverId);

        if (!sender || !receiver)
            throw new AppError("Sender or receiver not found", 404);
        if (!sender.is_active || !receiver.is_active) {
            throw new AppError(
                "Inactive accounts cannot perform transactions",
                400,
            );
        }
        if (Number(sender.balance) < Number(amount)) {
            throw new AppError("Insufficient balance", 400);
        }

        const updatedSender = await this.accountRepository.updateBalance(
            senderId,
            Number(sender.balance) - Number(amount),
        );

        const updatedReceiver = await this.accountRepository.updateBalance(
            receiverId,
            Number(receiver.balance) + Number(amount),
        );

        return {
            sender: toAccountResponse(updatedSender),
            receiver: toAccountResponse(updatedReceiver),
        };
    }
    async delete(id) {
        const deleted = await this.accountRepository.delete(id);
        if (!deleted) throw new AppError("Account not found", 404);
    }

    generateAccountNumber() {
        return `BNK-${Date.now()}-${Math.floor(1000 + Math.random() * 9000)}`;
    }
}

module.exports = AccountService;
