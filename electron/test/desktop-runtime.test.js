const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const net = require('node:net');

const {
  LOOPBACK_ADDRESS,
  createDesktopServerEnvironment,
  findAvailableLoopbackPort
} = require('../desktop-runtime');

test('desktop runtime always binds the bundled service to loopback', () => {
  const environment = createDesktopServerEnvironment(
    { EXISTING_VALUE: 'preserved', BIND_ADDRESS: '0.0.0.0' },
    48231,
    path.join('C:', 'Users', 'Example', 'AppData', 'Roaming', 'xiaozaclaw', 'data')
  );

  assert.equal(LOOPBACK_ADDRESS, '127.0.0.1');
  assert.equal(environment.EXISTING_VALUE, 'preserved');
  assert.equal(environment.PORT, '48231');
  assert.equal(environment.BIND_ADDRESS, '127.0.0.1');
  assert.equal(
    environment.SQLITE_PATH,
    path.join('C:', 'Users', 'Example', 'AppData', 'Roaming', 'xiaozaclaw', 'data', 'new-api.db')
  );
});

test('desktop runtime rejects invalid port and missing data directory', () => {
  assert.throws(() => createDesktopServerEnvironment({}, 0, 'C:\\data'), RangeError);
  assert.throws(() => createDesktopServerEnvironment({}, 65536, 'C:\\data'), RangeError);
  assert.throws(() => createDesktopServerEnvironment({}, 48231, ''), /data directory/);
});

test('desktop runtime allocates a usable loopback port instead of assuming port 3000', async () => {
  const port = await findAvailableLoopbackPort();
  const server = net.createServer();

  await new Promise((resolve, reject) => {
    server.once('error', reject);
    server.listen(port, LOOPBACK_ADDRESS, resolve);
  });

  assert.ok(Number.isInteger(port));
  assert.ok(port > 0);
  await new Promise((resolve) => server.close(resolve));
});
