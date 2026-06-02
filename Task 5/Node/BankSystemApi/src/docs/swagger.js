const path = require("path");
const swaggerJsdoc = require("swagger-jsdoc");

const options = {
    definition: {
        openapi: "3.0.0",
        info: {
            title: "Bank System API",
            version: "1.0.0",
            description:
                "Bank account API using Express, PostgreSQL, DI, DTOs, validation, and Swagger.",
        },
        servers: [
            {
                url: "http://localhost:3000",
            },
        ],
    },
    apis: [path.join(process.cwd(), "src/routes/*.js")],
};

const swaggerSpec = swaggerJsdoc(options);

module.exports = swaggerSpec;
