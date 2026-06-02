require("dotenv").config();

const { buildContainer } = require("./src/container");
const createApp = require("./src/app");

const { accountRoutes } = buildContainer();
const app = createApp({ accountRoutes });

const PORT = process.env.PORT || 3000;

app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});
