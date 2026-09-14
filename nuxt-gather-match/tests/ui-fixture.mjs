// Local, in-memory fixture for browser verification. Never connects to a database.
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { once } from 'node:events';

let activity;
const server = createServer(async (req, res) => {
  res.setHeader('Content-Type', 'application/json');
  const respond = (data, status = 200) => { res.statusCode = status; res.end(JSON.stringify({ success: status < 400, data, error: status >= 400 ? { message: 'Fixture request rejected' } : null })); };
  let raw = '';
  for await (const chunk of req) raw += chunk;
  const body = raw ? JSON.parse(raw) : {};
  if (req.url === '/api/reference-data') return respond({ activityTypes: [{ id: 1, name: '聚餐' }], cities: [{ id: 42, name: '臺北市', districts: [{ id: 102, name: '中山區' }] }] });
  if (req.url === '/api/auth/register') return respond({ id: 1 }, 201);
  if (req.url === '/api/auth/login') { res.setHeader('Set-Cookie', 'fixture=yes; Path=/; HttpOnly; SameSite=Lax'); return respond({ id: 1, displayName: '本機測試主揪', email: 'fixture@example.invalid' }); }
  if (req.url === '/api/auth/logout') { res.setHeader('Set-Cookie', 'fixture=; Path=/; Max-Age=0'); return respond({}); }
  if (!req.headers.cookie?.includes('fixture=yes')) return respond(null, 401);
  if (req.url === '/api/auth/me') return respond({ id: 1, displayName: '本機測試主揪', email: 'fixture@example.invalid' });
  try {
    if (req.url === '/api/activities' && req.method === 'POST') {
      assert.equal(body.cityId, 42);
      assert.equal(body.districtId, 102);
      assert.equal(body.activityTypeId, 1);
      assert.ok(body.deadlineAt.endsWith('Z'));
      assert.ok(body.dateOptions.length > 0);
      assert.ok(body.placeOptions[0].displayLabel);
      assert.equal(typeof body.budgetMin, 'number');
      activity = { ...body, id: 42, status: 'open', currencyCode: 'TWD', dateOptions: body.dateOptions.map((x, i) => ({ ...x, id: i + 11, startTime: null, endTime: null })), placeOptions: body.placeOptions.map((x, i) => ({ ...x, id: i + 21 })) };
      console.log('PASS: create payload uses numeric IDs, UTC deadline and candidate options');
      return respond({ id: 42, shareToken: 'LOCAL-FIXTURE-ONLY' }, 201);
    }
    if (req.url === '/api/activities/42' && activity) {
      if (req.method === 'PATCH') {
        assert.deepEqual(Object.keys(body).sort(), ['dateOptions', 'title']);
        assert.deepEqual(body.dateOptions.map(x => x.id), activity.dateOptions.map(x => x.id));
        assert.ok(body.dateOptions.every(x => !x.startTime || /^\d\d:\d\d:\d\d$/.test(x.startTime)));
        activity = { ...activity, ...body };
        console.log('PASS: update preserves option IDs and sends only editable fields');
      }
      return respond(activity);
    }
    respond(null, 404);
  } catch (error) { console.error(error.message); respond(null, 400); }
});
server.listen(0, '127.0.0.1');
await once(server, 'listening');
const child = spawn(process.execPath, ['.output/server/index.mjs'], {
  env: { ...process.env, PORT: '3001', HOST: '127.0.0.1', NUXT_API_BASE: `http://127.0.0.1:${server.address().port}` },
  stdio: 'inherit', windowsHide: true,
});
process.on('SIGINT', () => { child.kill(); server.close(); });
child.on('exit', () => { server.close(); });
