// URL του Web API
const apiUrlUsers = 'https://localhost:7121/api/users';  // Προσαρμόστε το URL ανάλογα με την τοποθεσία του API σας
const apiUrlRegister = 'https://localhost:7121/api/register';
const apiUrlHome = 'https://localhost:7121/api/home';

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
    const SecurityQuestion = document.getElementById('SecurityQuestion').value;
    const SecurityAnswer = document.getElementById('SecurityAnswer').value;

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




// Λήψη όλων των χρηστών
fetch(apiUrlUsers)
    .then(response => response.json())
    .then(data =>  {
     const usersList = document.getElementById('users-list');
     usersList.innerHTML = '';

     data.forEach(user => {
         const li = document.createElement('li');
         li.textContent = `${user.firstname}  ${user.lastname}`; 
         usersList.appendChild(li);
        
     });
}
);



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






