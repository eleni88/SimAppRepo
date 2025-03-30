// URLs
const apiUrlUsers    = 'https://localhost:7121/api/users'; 
const apiUrlRegister = 'https://localhost:7121/api/ath/register';
const apiUrlHome     = 'https://localhost:7121/api/home';
const apiUrlLogin    = 'https://localhost:7121/api/ath/login';

// Home
async function fetchWelcomeMessage() {
    try { 
        const response = await fetch(apiUrlHome); 
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        const data = await response.json();
        document.getElementById('message').innerText = data.message;
    } catch (error) {
        console.error("Error fetching data:", error);
        document.getElementById('message').innerText = "Failed to load message.";
    }
}

//Register
document.getElementById('register-form').addEventListener('submit', async function (event) {
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
        Password: Password,
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
            credentials: 'include',
            body: JSON.stringify(registeruser)
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || `HTTP error! Status: ${response.status}`);
        }

        document.getElementById('response-message').innerText = 'User registered successfully!';
    } catch (error) {
        document.getElementById('response-message').innerText = 'Error: ' + error.message;
    }
});

//Login
document.getElementById('login-form').addEventListener('submit', async function (event) {
    event.preventDefault();

    const userName = document.getElementById('UserName').value;
    const password = document.getElementById('Password').value;

    const loginuser = {
        UserName: userName,
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

        document.getElementById('response-message').innerText = 'User loggedin successfully!';
    } catch (error) {
        document.getElementById('response-message').innerText = 'Error: ' + error.message;
    }
});



// Λήψη όλων των χρηστών
document.addEventListener("DOMContentLoaded", async function () {
    try {
        const response = await fetch(apiUrlUsers);

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        const usersList = document.getElementById("users"); // Corrected (was 'users-list')
        usersList.innerHTML = ""; //  Clear previous list

        if (data.length == 0) {
            usersList.innerHTML = "<li>No users found.</li>"; // Handle empty response
            return;
        }

        data.forEach(user => {
            const li = document.createElement("li");
            li.textContent = `${user.firstname} ${user.lastname}`;
            usersList.appendChild(li);
        });
    } catch (error) {
        console.error("Error fetching users:", error);
        document.getElementById("users").innerHTML = "<li>Error loading users.</li>";
    }
});



//fetch(apiUrlUsers)
//    .then(response => response.json())
//    .then(data =>  {
//     const usersList = document.getElementById('users');
//     usersList.innerHTML = '';

//     data.forEach(user => {
//         const li = document.createElement('li');
//         li.textContent = `${user.firstname}  ${user.lastname}`; 
//         usersList.appendChild(li);
        
//     });
//}
//);



//-----------------------
// // Προσθήκη νέου χρήστη
document.getElementById('add-user-form').addEventListener('submit', function(event) {
    event.preventDefault();

    const userFirstName = document.getElementById('UserFirstName').value;
    const userLastName = document.getElementById('UserLastName').value;
    const userName = document.getElementById('UserName').value;
    const Password = document.getElementById('Password').value;
    const Email = document.getElementById('Email').value;
    const JobTitle = document.getElementById('JobTitle').value;
    const Age = document.getElementById('Age').value;
    const Admin = document.getElementById('Admin').value;

    const formuser = {
        firstname: userFirstName,
        lastname: userLastName,
        username: userName,
        Password: Password,
        email: Email,
        jobtitle: JobTitle,
        age: Age
       // Admin: Admin    
    };

    // const formuser = {
    //      userFirstName,
    //     userLastName,
    //      userName,
    //      Password,
    //     Email,
    //     JobTitle,
    //     Age,
    //     Admin    
    // };


    fetch(apiUrlUsers, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(formuser)
    })
    .then(response => response.json())
    .then(data => {
        document.getElementById('response-message').innerText = 'Form submitted successfully!';
    })
    .catch(error =>{
        document.getElementById('response-message').innerText = 'Error submitting form: ' + error.message;
    }); 
});






