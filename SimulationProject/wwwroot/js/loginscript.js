const apiUrlLogin    = 'https://localhost:7121/api/ath/login';

document.getElementById('login-form').addEventListener('submit', async function (event) {
    event.preventDefault();

    const userName = document.getElementById('UserName').value;
    const password = document.getElementById('Password').value;

    const loginuser = {
        Username: userName,
        Password: password
    };

    try {
        const response = await fetch(apiUrlLogin, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify(loginuser)
        });

        if (response.status == 204) {
            throw new Error("No content returned from server.");
        }

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || `HTTP error! Status: ${response.status}`);
        }
        else {
            document.getElementById('response-message').innerText = 'User loggedin successfully!';
            window.location.href = '/Home.html';
        }

    } catch (error) {
        document.getElementById('response-message').innerText = 'Error: ' + error.message;
    }
});