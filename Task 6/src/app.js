const express = require('express');
const sessionMiddleware = require('./middleware/session');
const routes = require('./routes');
const { errorPage } = require('./views/templates');

const app = express();

app.set('trust proxy', 1);
app.use(express.urlencoded({ extended: false }));
app.use(express.json());
app.use(sessionMiddleware);
app.use(routes);

app.use((error, req, res, next) => {
    console.error(error);
    res.status(500).send(errorPage(req.session?.account));
});

module.exports = app;
