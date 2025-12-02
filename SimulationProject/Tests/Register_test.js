import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';
import exec from 'k6/execution';

export const options = {
    scenarios: {
        register_scenario: {
            // name of the executor to use
            executor: 'per-vu-iterations', // each VU executes an exact number of iterations

            gracefulStop: '5s', // a duration that k6 will wait before forcefully interrupting an iteration

            vus: 50,
            iterations: 1,
            maxDuration: '2m',  // Maximum scenario duration before it’s forcibly stopped
        },
    },
    thresholds: {
        'http_req_failed': ['rate<0.01'],   // http errors should be less than 1%      
        'http_req_duration{ scenario: register_scenario }': ['p(90)<5000']  // 100% of requests have a response time < 5000ms  
    },
};

const RUN_ID = __ENV.RUN_ID 


export default function () {
    const i = exec.vu.idInTest;
    const username = `user${i}_${RUN_ID}`;

    const payload = JSON.stringify({
         username: `user${i}_${RUN_ID}`,
         password: `User${i}Password!@#123*${RUN_ID}`,
         firstname: `User${i}`,
         lastname: `User${i}`,
         email: `${username}@example.com`,
         admin: true,
         age: 30,
         jobtitle: 'Developer',
         active: true,
         organization: 'Lotr Corp',
         securityquestion: 'What city were you born in?',
         securityanswer: `Athens${i}!@#123`,
         securityquestion1: 'What was the first concert you attended?',
         securityanswer1: `Scorpions${i}!@#123`,
         securityquestion2: 'What was the make and model of your first car?',
         securityanswer2: `Corsa${i}!@#123`,
    });
    const url = 'http://127.0.0.1:8080/api/Ath/register'; //'https://localhost:7121/api/Ath/register';
    const params = {
        headers: { 'Content-Type': 'application/json' },
        timeout: '90s',
    };

    //localhost:7121
    // 127.0.0.1:8080
    const userregister = http.post(url, payload, params);

    if (userregister.status !== 200 && userregister.status !== 201) {
        console.error('Register failed:', userregister.status);
        console.error('Resp headers:', JSON.stringify(userregister.headers));
        console.error('user${i}_${RUN_ID}:', username);
        console.error('Body:', String(userregister.body).slice(0, 500));
    }

    check(userregister, { 'register success': (r) => r.status === 201 || r.status === 200,
    });

    sleep(1);
}