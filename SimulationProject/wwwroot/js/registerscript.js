const apiUrlRegister = 'https://localhost:7121/api/ath/register';

//Register
async function registerUser(event) {
    event.preventDefault();

    const FirstName = document.getElementById('FirstName').value;
    const LastName = document.getElementById('LastName').value;
    const userName = document.getElementById('UserName').value;
    const Password = document.getElementById('Password').value;
    const Email = document.getElementById('Email').value;
    const JobTitle = document.getElementById('JobTitle').value;
    const Age = document.getElementById('Age').value;
    const Admin = false;
    const SecurityQuestion = document.getElementById('SecurityQuestion1').value;
    const SecurityAnswer = document.getElementById('SecurityAnswer1').value;

    const registeruser = {
        firstname: FirstName,
        lastname: LastName,
        username: userName,
        password: Password, 
        email: Email,
        jobtitle: JobTitle,
        age: Age,
        Admin: Admin,
        SecurityQuestion: SecurityQuestion,
        SecurityAnswer: SecurityAnswer
    };

    try {
        const response = await fetch(apiUrlRegister, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(registeruser)
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || `HTTP error! Status: ${response.status}`);
        }

        document.getElementById('response-message').innerText = 'Registration successful';
        document.getElementById('response-message').style.color = 'green';
        window.location.href = '/Login.html';

    } catch (error) {
        document.getElementById('response-message').innerText = 'Error: ' + error.message;
        document.getElementById('response-message').style.color = 'red';
    }
}