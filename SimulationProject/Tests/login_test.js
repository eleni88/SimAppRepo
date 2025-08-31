import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';
import exec from 'k6/execution';

export const options = {
    vus: 50,
    duration: '2m',
};

const RUN_ID = __ENV.RUN_ID 

// The default exported function is gonna be picked up by k6 as the entry point for the test script. 
// It will be executed repeatedly in "iterations" for the whole duration of the test.
export default function () {
    const i = exec.vu.idInTest - 1;

    const username = `user${i}_${RUN_ID}`;
    const password = `User${i}Password!@#123*${RUN_ID}`;

    //1. Login
    const userlogin = http.post('https://localhost:7121/api/Ath/login',
        JSON.stringify({ username, password }),
        { headers: { 'Content-Type': 'application/json' } }
    );
    // makes sure the HTTP response code is a 200
    check(userlogin, { 'is status 200': (r) => r.status === 200,
    });

    //2. Get Users
    const getusers = http.get('https://localhost:7121/api/users');
    check(getusers, { 'GET 200': (r) => r.status === 200 });

    //3. Create Simulation
    const newSimulation = {
        Name: 'sim10',
        Description: 'sim10', 
        Codeurl: 'https://github.com/eleni88/MasterSlaveSimulation',
        Simparams:  '{ "simulations": [ { "interArrivalTime": 1.0, "serviceTime": 2.0, "serverCapacity": 1, },{ "interArrivalTime": 1.5, "serviceTime": 1.0, "serverCapacity": 2, "queueCapacity": 10, "duration": 40 } ] }',
        Simcloud: 1,
        Regionid: 1,
        Resourcerequirement: {
            Instancetype: 'Pod',
            Mininstances: 1,
            Maxinstances: 5
        },
        Simuser: user,
        Createdate: new Date().toISOString()
    };

    const simulationcreate = http.post('https://localhost:7121/api/simulations/create',
        JSON.stringify(newSimulation),
        { headers: { 'Content-Type': 'application/json' } }
    );

    check(simulationcreate, {
        'POST created': (r) => r.status === 201 || r.status === 200,
    });

    // Sleep for 1 second to simulate real-world usage
    sleep(1);
}