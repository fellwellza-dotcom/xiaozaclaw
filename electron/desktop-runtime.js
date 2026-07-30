const path = require('path');
const net = require('net');

const LOOPBACK_ADDRESS = '127.0.0.1';

function createDesktopServerEnvironment(parentEnvironment, port, dataDir) {
  if (!Number.isInteger(port) || port < 1 || port > 65535) {
    throw new RangeError('Desktop server port must be an integer between 1 and 65535.');
  }

  if (!dataDir) {
    throw new Error('Desktop data directory is required.');
  }

  return {
    ...parentEnvironment,
    PORT: String(port),
    BIND_ADDRESS: LOOPBACK_ADDRESS,
    SQLITE_PATH: path.join(dataDir, 'new-api.db')
  };
}

function findAvailableLoopbackPort() {
  return new Promise((resolve, reject) => {
    const reservation = net.createServer();
    reservation.unref();
    reservation.once('error', reject);
    reservation.listen(0, LOOPBACK_ADDRESS, () => {
      const address = reservation.address();
      reservation.close((error) => {
        if (error) {
          reject(error);
          return;
        }
        resolve(address.port);
      });
    });
  });
}

module.exports = {
  LOOPBACK_ADDRESS,
  createDesktopServerEnvironment,
  findAvailableLoopbackPort
};
