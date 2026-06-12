const express = require('express');
const requireAuth = require('../middleware/requireAuth');
const authService = require('../services/authService');
const {
    homePage,
    loginPage,
    dashboardPage,
    profilePage,
} = require('../views/templates');

const router = express.Router();

router.get('/', (req, res) => {
    res.send(homePage(req.session.account));
});

router.get('/login', (req, res) => {
    res.send(loginPage(req.session.account));
});

router.get('/auth/microsoft', async (req, res, next) => {
    try {
        const url = await authService.getAuthCodeUrl(req.session);
        res.redirect(url);
    } catch (error) {
        next(error);
    }
});

router.get('/auth/callback', async (req, res, next) => {
    try {
        if (!req.query.code) {
            return res.status(400).send('Missing authorization code.');
        }

        if (!req.query.state || req.query.state !== req.session.authState) {
            return res.status(400).send('Invalid state parameter.');
        }

        const result = await authService.getTokenByCode(req.query.code);
        req.session.account = result.account;
        req.session.idTokenClaims = result.idTokenClaims || {};
        delete req.session.authState;
        res.redirect('/dashboard');
    } catch (error) {
        next(error);
    }
});

router.get('/dashboard', requireAuth, (req, res) => {
    res.send(
        dashboardPage(req.session.account, req.session.idTokenClaims || {})
    );
});

router.get('/profile', requireAuth, (req, res) => {
    res.send(profilePage(req.session.account, req.session.idTokenClaims || {}));
});

router.get('/logout', (req, res) => {
    const logoutUrl = authService.getLogoutUrl();
    req.session.destroy(() => {
        res.redirect(logoutUrl);
    });
});

module.exports = router;
