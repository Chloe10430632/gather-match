import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { once } from 'node:events';

test('production proxy preserves cookies, bodies, errors and rejects cross-origin writes', async () => {
  const requests = [];
  const upstream = createServer(async (req, res) => {
    let body = '';
    for await (const chunk of req) body += chunk;
    requests.push({ path: req.url, method: req.method, cookie: req.headers.cookie, body });
    res.setHeader('Content-Type', 'application/json');
    if (req.url === '/api/auth/login') {
      res.setHeader('Set-Cookie', ['session=test-only; HttpOnly; Path=/; SameSite=Lax', 'second=test; Path=/']);
    }
    if (req.url === '/api/auth/logout') res.setHeader('Set-Cookie', 'session=; Max-Age=0; Path=/');
    if (req.url === '/api/activities/999') res.statusCode = 404;
    if (req.url === '/api/activities' && req.method === 'POST') res.statusCode = 201;
    res.end(JSON.stringify({ success: res.statusCode < 400, data: { id: 42 }, error: res.statusCode === 404 ? { code: 'not_found' } : null }));
  });
  upstream.listen(0, '127.0.0.1');
  await once(upstream, 'listening');
  // Reserve a free local port for Nitro.
  const reservation = createServer();
  reservation.listen(0, '127.0.0.1');
  await once(reservation, 'listening');
  const port = reservation.address().port;
  await new Promise(resolve => reservation.close(resolve));
  const child = spawn(process.execPath, ['.output/server/index.mjs'], {
    env: { ...process.env, PORT: String(port), HOST: '127.0.0.1', NUXT_API_BASE: `http://127.0.0.1:${upstream.address().port}` },
    stdio: ['ignore', 'pipe', 'pipe'], windowsHide: true,
  });
  try {
    await new Promise((resolve, reject) => {
      const timer = setTimeout(() => reject(new Error('Nitro startup timeout')), 15000);
      child.stdout.on('data', chunk => { if (String(chunk).includes('Listening')) { clearTimeout(timer); resolve(); } });
      child.once('error', reject);
      child.once('exit', code => { clearTimeout(timer); reject(new Error(`Nitro exited: ${code}`)); });
    });
    const base = `http://127.0.0.1:${port}`;
    const headers = { 'x-gather-match': '1', 'content-type': 'application/json', origin: base };
    const login = await fetch(`${base}/api/auth/login`, { method: 'POST', headers, body: '{"email":"test@example.invalid","password":"test-only"}' });
    assert.equal(login.status, 200);
    assert.equal(login.headers.get('cache-control'), 'no-store');
    assert.equal(login.headers.getSetCookie().length, 2);
    assert.match(login.headers.getSetCookie()[0], /HttpOnly/);
    const cookie = 'session=test-only';
    await fetch(`${base}/api/auth/me`, { headers: { cookie } });
    assert.equal(requests.at(-1).cookie, cookie);
    const body = { title: '繁體中文活動', dateOptions: [{ id: 11, optionDate: '2026-10-01', startTime: '18:30:00', endTime: null }] };
    const created = await fetch(`${base}/api/activities`, { method: 'POST', headers: { ...headers, cookie }, body: JSON.stringify(body) });
    assert.equal(created.status, 201);
    await fetch(`${base}/api/activities/42`, { method: 'PATCH', headers: { ...headers, cookie }, body: JSON.stringify(body) });
    assert.deepEqual(JSON.parse(requests.at(-1).body), body);
    assert.equal(requests.at(-1).method, 'PATCH');
    assert.equal(requests.at(-1).cookie, cookie);
    const missing = await fetch(`${base}/api/activities/999`);
    assert.equal(missing.status, 404);
    assert.equal((await missing.json()).error.code, 'not_found');
    const before = requests.length;
    for (const invalidHeaders of [{}, { ...headers, origin: 'https://evil.example' }, { ...headers, 'sec-fetch-site': 'cross-site' }]) {
      assert.equal((await fetch(`${base}/api/auth/logout`, { method: 'POST', headers: invalidHeaders, body: '{}' })).status, 403);
    }
    assert.equal((await fetch(`${base}/api/admin`)).status, 404);
    assert.equal(requests.length, before);
    const logout = await fetch(`${base}/api/auth/logout`, { method: 'POST', headers, body: '{}' });
    assert.match(logout.headers.get('set-cookie'), /Max-Age=0/);
  } finally {
    child.kill();
    await once(child, 'exit');
    upstream.closeAllConnections();
    await new Promise(resolve => upstream.close(resolve));
  }
});
