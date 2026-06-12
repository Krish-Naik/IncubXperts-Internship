function escapeHtml(value = '') {
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function layout({ title, body, user }) {
    return `<!doctype html>
    <html lang="en">
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>${escapeHtml(title)}</title>
        <style>
            * { 
                box-sizing: border-box; 
            }
            body {
                margin: 0;
                font-family: Arial, Helvetica, sans-serif;
                background: #f7f7f7;
                color: #222;
            }
            .container {
                max-width: 920px;
                margin: 0 auto;
                padding: 24px 16px 48px;
            }
            .nav {
                display: flex;
                justify-content: space-between;
                align-items: center;
                gap: 12px;
                padding: 12px 0 24px;
                flex-wrap: wrap;
            }
            .nav-links, .actions {
                display: flex;
                gap: 10px;
                flex-wrap: wrap;
            }
            .btn {
                display: inline-block;
                padding: 10px 16px;
                border-radius: 6px;
                border: 1px solid #d1d5db;
                background: white;
                color: #111827;
                text-decoration: none;
                font-size: 14px;
            }
            .btn-primary {
                background: #2563eb;
                color: white;
                border-color: #2563eb;
            }
            .card {
                background: white;
                border: 1px solid #e5e7eb;
                border-radius: 10px;
                padding: 24px;
                margin-bottom: 20px;
            }
            h1 {
                margin-top: 0;
                font-size: 32px;
                line-height: 1.2;
                color: #111827;
            }
            h2 {
                margin-top: 0;
                font-size: 22px;
                color: #111827;
            }
            p {
                line-height: 1.6;
                color: #374151;
            }
            .grid {
                display: grid;
                grid-template-columns: repeat(2, minmax(0, 1fr));
                gap: 16px;
                margin-top: 16px;
            }
            .info {
                background: #f9fafb;
                border: 1px solid #e5e7eb;
                border-radius: 8px;
                padding: 16px;
            }
            .info strong {
                display: block;
                margin-bottom: 8px;
                color: #111827;
            }
            ul {
                padding-left: 20px;
                color: #374151;
                line-height: 1.6;
            }
            @media (max-width: 700px) {
                .grid {
                    grid-template-columns: 1fr;
                }
            }
        </style>
    </head>
    <body>
        <div class="container">
            <div class="nav">
                <div class="nav-links">
                    <a class="btn" href="/">Home</a>
                    ${user ? '<a class="btn" href="/dashboard">Dashboard</a><a class="btn" href="/profile">Profile</a><a class="btn btn-primary" href="/logout">Logout</a>' : '<a class="btn" href="/login">Login</a>'}
                </div>
            </div>
            ${body}
        </div>
    </body>
    </html>`;
}

function homePage(user) {
    return layout({
        title: 'Home',
        user,
        body: `
        <div class="card">
            <h1>Landing Page</h1>
            <p>This is a simple landing page for testing Microsoft Entra authentication properly.</p>
            <div class="actions">
                <a class="btn btn-primary" href="/login">Go to Login</a>
            </div>
        </div>
        <div class="card">
            <h2>How this app works</h2>
            <ul>
                <li>You open a normal landing page first.</li>
                <li>You click login from your app.</li>
                <li>The app redirects to Microsoft for secure sign-in.</li>
                <li>After success, Microsoft returns to <code>/auth/callback</code>.</li>
                <li>The app creates a session and sends you to the dashboard.</li>
            </ul>
        </div>
    `,
    });
}

function loginPage(user) {
    return layout({
        title: 'Login',
        user,
        body: `
        <div class="card">
            <h1>Login</h1>
            <p>Use your Microsoft organization account to sign in. The actual sign-in form is hosted by Microsoft for security, and you will come back to this app after login.</p>
            <div class="actions">
                <a class="btn btn-primary" href="/auth/microsoft">Continue with Microsoft</a>
                <a class="btn" href="/">Back to Home</a>
            </div>
        </div>
    `,
    });
}

function dashboardPage(account, claims) {
    return layout({
        title: 'Dashboard',
        user: account,
        body: `
        <div class="card">
            <h1>Dashboard</h1>
            <p>You are signed in successfully and the session is working.</p>
            <div class="grid">
                <div class="info"><strong>Name</strong><p>${escapeHtml(account?.name || 'Not available')}</p></div>
                <div class="info"><strong>Username</strong><p>${escapeHtml(account?.username || 'Not available')}</p></div>
                <div class="info"><strong>Tenant ID</strong><p>${escapeHtml(account?.tenantId || 'Not available')}</p></div>
                <div class="info"><strong>OID / Subject</strong><p>${escapeHtml(claims?.oid || claims?.sub || 'Not available')}</p></div>
            </div>
        </div>
    `,
    });
}

function profilePage(account, claims) {
    const claimRows = Object.entries(claims || {})
        .map(
            ([key, value]) =>
                `<div class="info"><strong>${escapeHtml(key)}</strong><p>${escapeHtml(typeof value === 'string' ? value : JSON.stringify(value))}</p></div>`
        )
        .join('');

    return layout({
        title: 'Profile',
        user: account,
        body: `
        <div class="card">
            <h1>Profile</h1>
            <p>This page shows the authenticated account data and ID token claims.</p>
            <div class="grid">
                <div class="info"><strong>Name</strong><p>${escapeHtml(account?.name || 'Not available')}</p></div>
                <div class="info"><strong>Username</strong><p>${escapeHtml(account?.username || 'Not available')}</p></div>
                ${claimRows || '<div class="info"><strong>No claims</strong><p>No additional claims were found.</p></div>'}
            </div>
        </div>
    `,
    });
}

function errorPage(user) {
    return layout({
        title: 'Error',
        user,
        body: `
        <div class="card">
            <h1>Something went wrong</h1>
            <p>Please check your terminal logs and verify that your Azure redirect URI and local route match exactly.</p>
            <div class="actions">
                <a class="btn" href="/">Home</a>
                <a class="btn btn-primary" href="/login">Login</a>
            </div>
        </div>
    `,
    });
}

module.exports = {
    homePage,
    loginPage,
    dashboardPage,
    profilePage,
    errorPage,
};
