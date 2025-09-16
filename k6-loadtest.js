import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 20,
  duration: '30s',
};

const API = __ENV.API || 'http://localhost:8080';
const KEY = __ENV.KEY || 'dev-key';

export default function () {
  const res = http.get(`${API}/health`, { headers: { 'X-API-Key': KEY } });
  check(res, { 'status 200': (r) => r.status === 200 });
  sleep(0.2);
}
