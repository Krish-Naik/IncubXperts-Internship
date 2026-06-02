const express = require("express");
const swaggerUi = require("swagger-ui-express");
const swaggerSpec = require("./docs/swagger");
const errorMiddleware = require("./middlewares/error.middleware");

function createApp({ accountRoutes }) {
    const app = express();

    app.use(express.json());
    app.use(express.urlencoded({ extended: true }));
    app.use("/api/docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));
    app.use("/api/accounts", accountRoutes);
    app.use(errorMiddleware);

    return app;
}

module.exports = createApp;
