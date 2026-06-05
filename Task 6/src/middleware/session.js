const session = require('express-session');
const env = require('../config/env');

module.exports = session({
    name: 'entra.sid',
    secret: env.expressSessionSecret,
    resave: false,
    saveUninitialized: false,
    rolling: true,
    cookie: {
        httpOnly: true,
        secure: env.isProd,
        sameSite: 'lax',
        maxAge: 1000 * 60 * 60,
    },
});
