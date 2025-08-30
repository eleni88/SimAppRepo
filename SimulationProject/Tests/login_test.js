import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';

export const options = {
    vus: 50,
    duration: '5m',
};

// array of users
const users = [];
for (let i = 1; i <= 50; i++) {
    users.push({
        username: `user${i}`,
        password: `User${i}Password!@#123`
    });
}

// The default exported function is gonna be picked up by k6 as the entry point for the test script. 
// It will be executed repeatedly in "iterations" for the whole duration of the test.
export default function () {
    // Login URL
    const user = users[(__VU - 1) % users.length];

    const res = http.post('https://127.0.0.1:5500/api/ath/login',
        JSON.stringify({ username: user.username, password: user.password }),
        { headers: { 'Content-Type': 'application/json' } }
    );

    // makes sure the HTTP response code is a 200
    check(res, { 'is status 200': (r) => r.status === 200,
    });

    // Sleep for 1 second to simulate real-world usage
    sleep(1);
}