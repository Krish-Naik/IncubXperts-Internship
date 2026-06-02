const { createDbPool } = require("./config/db");
const AccountRepository = require("./repositories/account.repository");
const AccountService = require("./services/account.service");
const AccountController = require("./controllers/account.controller");
const createAccountRoutes = require("./routes/account.routes");

function buildContainer() {
    const db = createDbPool();
    const accountRepository = new AccountRepository(db);
    const accountService = new AccountService(accountRepository);
    const accountController = new AccountController(accountService);
    const accountRoutes = createAccountRoutes(accountController);

    return { db, accountRoutes };
}

module.exports = { buildContainer };
