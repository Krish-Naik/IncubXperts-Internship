function normalizeType(type) {
    return String(type || "")
        .trim()
        .toUpperCase();
}

function validateCreateAccount(body) {
    const errors = [];

    if (!body.holderName || body.holderName.trim().length < 3) {
        errors.push("holderName must be at least 3 characters");
    }

    if (!body.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(body.email)) {
        errors.push("email must be valid");
    }

    const type = normalizeType(body.type);
    if (!["SAVINGS", "CURRENT"].includes(type)) {
        errors.push("type must be SAVINGS or CURRENT");
    }

    if (body.openingBalance === undefined || Number(body.openingBalance) < 0 || Number.isNaN(Number(body.openingBalance))) {
        errors.push("openingBalance must be a number and 0 or more");
    }

    return errors;
}

function validateAmount(amount) {
    return Number(amount) > 0;
}

module.exports = {
    normalizeType,
    validateCreateAccount,
    validateAmount,
};
