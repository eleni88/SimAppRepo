import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';
import exec from 'k6/execution';

export const options = {
    scenarios:
    {
        user_scenario: {
            executor: 'per-vu-iterations',

            gracefulStop: '5s',

            vus: 50,
            iterations: 1,
            maxDuration: '2m',
        },
    },

    thresholds: {
        'http_req_failed{name:user_login}': ['rate<0.01'],   // http errors should be less than 1%
        'http_req_failed{name:get_users}': ['rate<0.01'],   // http errors should be less than 1%
        'http_req_failed{name:simulation_create}': ['rate<0.01'],   // http errors should be less than 1%
        'http_req_duration{name:user_login}': ['p(100)<5000'],  // 100% των requests < 5000ms  
        'http_req_duration{name:get_users}': ['p(100)<5000'],  // 100% των requests < 5000ms
        'http_req_duration{name:simulation_create}': ['p(100)<5000'],  // 100% των requests < 5000ms
    },
};

const RUN_ID = __ENV.RUN_ID 

// The default exported function is gonna be picked up by k6 as the entry point for the test script. 
// It will be executed repeatedly in "iterations" for the whole duration of the test.
export default function () {
    const i = exec.vu.idInTest;

    const username = `user${i}_${RUN_ID}`;
    const password = `User${i}Password!@#123*${RUN_ID}`;

    //1. Login
    const userlogin = http.post('http://127.0.0.1:8080/api/Ath/login',  //localhost:7121 ,   127.0.0.1:8080
        JSON.stringify({ username, password }),
        {
            headers: { 'Content-Type': 'application/json' },
            tags: { name: 'user_login' }
        }
    );

    console.log('Set-Cookie keys:', Object.keys(userlogin.cookies || {}));
    console.log('Cookies detail:', JSON.stringify(userlogin.cookies, null, 2));

    if (userlogin.status !== 200) {
        console.error('LOGIN failed', userlogin.status, String(userlogin.body).slice(0, 200));
    }
    // makes sure the HTTP response code is a 200
    check(userlogin, { 'is status 200': (r) => r.status === 200,
    });

    //2. Get Users
    const getusers = http.get('http://127.0.0.1:8080/api/Users',
        { tags: { name: 'get_users' } });

    if (getusers.status !== 200) {
        console.error('GET /users failed', getusers.status, String(getusers.body).slice(0, 200));
    }

    check(getusers, { 'GET 200': (r) => r.status === 200 });

    //3. Create Simulation
    const body = userlogin.json();
    const newSimulation = {
        Name: 'sim10',
        Description: 'sim10', 
        Codeurl: 'https://github.com/eleni88/MasterSlaveSimulation',
        Simparams: JSON.stringify({
            simulations: [
                {
                    interArrivalTime: 1.0,
                    serviceTime: 2.0,
                    serverCapacity: 1,
                    queueCapacity: 5,
                    duration: 30
                },
                {
                    interArrivalTime: 1.5,
                    serviceTime: 1.0,
                    serverCapacity: 2,
                    queueCapacity: 10,
                    duration: 40
                }]
        }),
        Simcloud: 1,
        Regionid: 1,
        Resourcerequirement: {
            Instancetype: 'Pod',
            Mininstances: 1,
            Maxinstances: 5
        }
    };

    const simulationcreate = http.post('http://127.0.0.1:8080/api/Simulation/create',
        JSON.stringify(newSimulation),
        {
            headers: { 'Content-Type': 'application/json' },
            tags: { name: 'simulation_create' }
        }
    );

    const bodyStr = JSON.stringify(simulationcreate);
    console.log('Payload:', bodyStr);

    if (simulationcreate.status !== 201 && simulationcreate.status !== 200) {
        console.error('POST /simulations/create failed', simulationcreate.status, String(simulationcreate.body).slice(0, 200));
    }

    check(simulationcreate, {
        'POST created': (r) => r.status === 201 || r.status === 200,
    });

    // Sleep for 1 second to simulate real-world usage
    sleep(1);
}