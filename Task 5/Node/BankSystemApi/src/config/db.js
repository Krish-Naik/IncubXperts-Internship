const { Pool } = require("pg");

function createDbPool() {
    return new Pool({
        connectionString: process.env.DATABASE_URL,
    });
}

module.exports = { createDbPool };
