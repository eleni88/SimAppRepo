const apiUrlUsers    = 'https://localhost:7121/api/users';

document.addEventListener("DOMContentLoaded", async function () {
    try {
        const response = await fetch(apiUrlUsers, {
            method: "GET",
            credentials: "include", 
            headers: {
                "Content-Type": "application/json"
            },
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        const usersList = document.getElementById("users"); 
        usersList.innerHTML = ""; 

        if (!Array.isArray(data) || data.length === 0) {
            usersList.innerHTML = "<li>No users found.</li>";
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