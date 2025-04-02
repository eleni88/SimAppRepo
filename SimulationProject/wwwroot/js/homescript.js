const apiUrlHome     = 'https://localhost:7121/api/home';

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