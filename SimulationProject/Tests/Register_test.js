import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';
import exec from 'k6/execution';

export const options = {
    scenarios: {
        register_scenario: {
            // name of the executor to use
            executor: 'per-vu-iterations',

            gracefulStop: '5s',

            vus: 50,
            iterations: 1,
            maxDuration: '2m',
        },
    },
    thresholds: {
        http_req_failed: ['rate<0.01'],   // http errors should be less than 1%      
        http_req_duration: ['p(95)<200'], // 95% of requests should be below 200ms   
    },
};

const RUN_ID = __ENV.RUN_ID 

export default function () {
    const i = exec.vu.idInTest;
    const username = `user${i}_${RUN_ID}`;
    const password = `User${i}Password!@#123*${RUN_ID}`;
    const firstname = `User${i}`;
    const lastname = `User${i}`;
    const email = `${username}@example.com`;
    const admin = false;
    const age = 30;
    const jobtitle = 'Developer';
    const active = true;
    const organization = 'Lotr Corp';
    const securityquestion = 'What city were you born in?';
    const securityanswer = `Athens${i}!@#123`;
    const securityquestion1 = 'What was the first concert you attended?';
    const securityanswer1 = `Scorpions${i}!@#123`;
    const securityquestion2 = 'What was the make and model of your first car?';
    const securityanswer2 = `Corsa${i}!@#123`;

    const payload = JSON.stringify({
        username: username,
        password: password,
        firstname: firstname,
        lastname: lastname, 
        email: email,
        admin: admin,
        age: age,
        jobtitle: jobtitle,
        active: active,
        organization: organization,
        securityQuestion: securityquestion,
        securityAnswer: securityanswer,
        securityQuestion1: securityquestion1,
        securityAnswer1: securityanswer1,
        securityQuestion2: securityquestion2,
        securityAnswer2: securityanswer2,       
    });

    const userregister = http.post('https://localhost:7121/api/Ath/register',
        payload,
        { headers: { 'Content-Type': 'application/json' } }
    );

    if (userregister.status !== 200 && userregister.status !== 201) {
        console.error('Register failed:', userregister.status);
        console.error('Resp headers:', JSON.stringify(userregister.headers));

        console.error('Body:', String(userregister.body).slice(0, 500));
    }

    check(userregister, { 'register success': (r) => r.status === 201 || r.status === 200,
    });

    sleep(1);
}