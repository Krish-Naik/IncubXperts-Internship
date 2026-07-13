module.exports = function requireAuth(req, res, next) {
    if (!req.session.account) {
        return res.redirect('/login');
    }
    next();
};
