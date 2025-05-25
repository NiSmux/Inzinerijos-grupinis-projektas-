import http from 'k6/http';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';

// Bendras API bazinis URL
const BASE_URL = 'http://localhost:5293'; // Pakeiskite į savo API URL ir portą!

// Globalios konfigūracijos apkrovos testui
export const options = {
    vus: 1000,          // Virtualūs vartotojai. Testavimas atliktas su 50 VU.
    duration: '5m',   // Testo trukmė: 5 minutės
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% užklausų atsako laikas turi būti < 500ms
        http_req_failed: ['rate<0.01'],   // Klaidų lygis turi būti mažesnis nei 1%
    },
};

// Sukuriami vartotojo duomenys. Naudojame SharedArray, kad duomenys būtų sukurti
// tik vieną kartą ir dalinami tarp visų VUs.
const users = new SharedArray('users', function () {
    const data = [];
    // Generuojame pakankamai unikalių vartotojų, kad išvengtume pasikartojimų
    const numUsersToGenerate = 1000; 
    
    for (let i = 0; i < numUsersToGenerate; i++) {
        data.push({
            name: `Test User ${i}`,
            email: `testuser4_${i}@example.com`, // Unikalus el. paštas kiekvienam VU
            password: 'Password123!',
        });
    }
    return data;
});

// Pagrindinė testavimo funkcija, kurią vykdo kiekvienas virtualus vartotojas
export default function () {
    const user = users[__VU]; 
    
    if (!user) {
        console.error(`VU ${__VU}: Nerasta vartotojo duomenų. Tai reiškia, kad SharedArray nesugeneravo pakankamai vartotojų visoms VUs.`);
        sleep(1); 
        return; 
    }

    const registerPayload = JSON.stringify({
        name: user.name,
        email: user.email,
        password: user.password,
        confirmPassword: user.password,
    });

    const loginPayload = JSON.stringify({
        email: user.email,
        password: user.password,
    });

    let authToken = null;
    let boardId = null;
    let taskId = null;

    // 1. Sukuriamas naujas vartotojas (Register)
    const registerRes = http.post(`${BASE_URL}/api/auth/register`, registerPayload, {
        headers: { 'Content-Type': 'application/json' },
    });

    if (!check(registerRes, { 
        'Registracija: Statusas 200 (OK) arba 400 (El. paštas jau egzistuoja)': (r) => r.status === 200 || 
                                                                    (r.status === 400 && r.body.includes("Email already exists."))
    })) {
        console.error(`VU ${__VU}: Registracijos patikra nepavyko ${user.email}. Statusas: ${registerRes.status}, Atsakas: ${registerRes.body}, Trukmė: ${registerRes.timings.duration.toFixed(2)}ms`);
        sleep(1); 
        return; 
    }
    sleep(1);

    // 2. Prisijungiame prie paskyros (Login)
    const loginRes = http.post(`${BASE_URL}/api/auth/login`, loginPayload, {
        headers: { 'Content-Type': 'application/json' },
    });

    if (!check(loginRes, { 'Prisijungimas: Statusas 200 (OK)': (r) => r.status === 200 })) {
        console.error(`VU ${__VU}: Prisijungimas nepavyko ${user.email}. Statusas: ${loginRes.status}, Atsakas: ${loginRes.body}, Trukmė: ${loginRes.timings.duration.toFixed(2)}ms`);
        sleep(1); 
        return; 
    }
    
    if (loginRes.status === 200) {
        authToken = loginRes.json().token; 
        if (!authToken) {
            console.error(`VU ${__VU}: Prisijungimas sėkmingas (200), bet negautas joks tokenas ${user.email}. Atsakas: ${loginRes.body}, Trukmė: ${loginRes.timings.duration.toFixed(2)}ms`);
            sleep(1);
            return;
        }
    } else {
        console.error(`VU ${__VU}: Netikėtas prisijungimo gedimas ${user.email}. Statusas: ${loginRes.status}, Atsakas: ${loginRes.body}, Trukmė: ${loginRes.timings.duration.toFixed(2)}ms`);
        sleep(1);
        return;
    }
    sleep(1);

    const authHeaders = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authToken}`,
        },
    };

    // 3. Sukuriama nauja užduočių lenta (Create Board)
    const createBoardPayload = JSON.stringify({
        title: `Test Board from K6 VU ${__VU} ${new Date().toISOString()}`,
    });
    const createBoardRes = http.post(`${BASE_URL}/api/boards`, createBoardPayload, authHeaders);
    if (!check(createBoardRes, { 'Sukurti lentą: Statusas 201 (Created)': (r) => r.status === 201 })) {
        console.error(`VU ${__VU}: Lentos kūrimas nepavyko. Statusas: ${createBoardRes.status}, Atsakas: ${createBoardRes.body}, Trukmė: ${createBoardRes.timings.duration.toFixed(2)}ms`);
        sleep(1);
        return;
    }
    boardId = createBoardRes.json()?.id;
    if (!boardId) {
        console.error(`VU ${__VU}: Lenta sukurta sėkmingai (201), bet negautas lentos ID. Atsakas: ${createBoardRes.body}, Trukmė: ${createBoardRes.timings.duration.toFixed(2)}ms`);
        sleep(1);
        return;
    }
    sleep(1);

    // 4. Pridedama nauja užduotis (Add Task)
    const addTaskPayload = JSON.stringify({
        title: `Task from K6 VU ${__VU} ${new Date().toISOString()}`,
        description: 'This is a test task.',
        status: 'Todo',
        boardId: boardId,
    });
    const addTaskRes = http.post(`${BASE_URL}/api/tasks`, addTaskPayload, authHeaders);
    if (!check(addTaskRes, { 'Pridėti užduotį: Statusas 201 (Created)': (r) => r.status === 201 })) {
        console.error(`VU ${__VU}: Užduoties pridėjimas nepavyko. Statusas: ${addTaskRes.status}, Atsakas: ${addTaskRes.body}, Trukmė: ${addTaskRes.timings.duration.toFixed(2)}ms`);
        sleep(1);
        return;
    }
    taskId = addTaskRes.json()?.id;
    if (!taskId) {
        console.error(`VU ${__VU}: Užduotis pridėta sėkmingai (201), bet negautas užduoties ID. Atsakas: ${addTaskRes.body}, Trukmė: ${addTaskRes.timings.duration.toFixed(2)}ms`);
        sleep(1);
        return;
    }
    sleep(1);

    // 5. Pakeičiame užduoties statusą (Update Task Status)
    const updateStatusPayload = JSON.stringify({
        id: taskId,
        title: `Task from K6 VU ${__VU} (Atnaujintas statusas)`, 
        description: 'This is a test task.',
        status: 'InProgress',
        boardId: boardId,
    });
    const updateStatusRes = http.put(`${BASE_URL}/api/tasks/${taskId}`, updateStatusPayload, authHeaders);
    if (!check(updateStatusRes, { 'Atnaujinti užduoties statusą: Statusas 204 (No Content)': (r) => r.status === 204 })) {
        console.error(`VU ${__VU}: Užduoties statuso atnaujinimas nepavyko. Statusas: ${updateStatusRes.status}, Atsakas: ${updateStatusRes.body}, Trukmė: ${updateStatusRes.timings.duration.toFixed(2)}ms`);
        sleep(1);
        return;
    }
    sleep(1);

    // 6. Pakeičia užduoties tekstą (Update Task Text)
    const updateTextPayload = JSON.stringify({
        id: taskId,
        title: `Task from K6 VU ${__VU} (Atnaujintas tekstas) ${new Date().toISOString()}`,
        description: 'This is an updated description.',
        status: 'InProgress',
        boardId: boardId,
    });
    const updateTextRes = http.put(`${BASE_URL}/api/tasks/${taskId}`, updateTextPayload, authHeaders);
    if (!check(updateTextRes, { 'Atnaujinti užduoties tekstą: Statusas 204 (No Content)': (r) => r.status === 204 })) {
        console.error(`VU ${__VU}: Užduoties teksto atnaujinimas nepavyko. Statusas: ${updateTextRes.status}, Atsakas: ${updateTextRes.body}, Trukmė: ${updateTextRes.timings.duration.toFixed(2)}ms`);
        sleep(2);
        return;
    }
    sleep(2);
}