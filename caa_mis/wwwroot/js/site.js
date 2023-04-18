// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function generateSKU()
{
    // Generate random SKU generation that start with CAA and between 0 and 999999
    var sku = "CAA" + Math.floor(Math.random() * 1000000);
    document.getElementById("SKUNumber").value = sku;    
}


let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
    // Prevent Chrome 67 and earlier from automatically showing the prompt
    e.preventDefault();

    // Stash the event so it can be triggered later
    deferredPrompt = e;

    // Show the install button
    const installButton = document.getElementById('installButton');
    if (installButton) {
        installButton.style.display = 'block';
    }
});

const installButton = document.getElementById('installButton');
if (installButton) {
    installButton.addEventListener('click', (e) => {
        // Hide the button after the click
        e.target.style.display = 'none';

        // Show the deferred install prompt
        if (deferredPrompt) {
            deferredPrompt.prompt();

            // Wait for the user to respond to the prompt
            deferredPrompt.userChoice
                .then((choiceResult) => {
                    if (choiceResult.outcome === 'accepted') {
                        console.log('User accepted the install prompt');
                    } else {
                        console.log('User dismissed the install prompt');
                    }
                    deferredPrompt = null;
                });
        }
    });
}

function showInstallPrompt() {
    // Create an install button or make an existing button visible
    const installButton = document.createElement('button');
    installButton.id = 'installButton';
    installButton.textContent = 'Install App';
    document.body.appendChild(installButton);

    // Add a click event listener to the button
    installButton.addEventListener('click', (e) => {
        // Hide the button after the click
        installButton.style.display = 'none';

        // Show the deferred install prompt
        if (deferredPrompt) {
            deferredPrompt.prompt();

            // Wait for the user to respond to the prompt
            deferredPrompt.userChoice
                .then((choiceResult) => {
                    if (choiceResult.outcome === 'accepted') {
                        console.log('User accepted the install prompt');
                    } else {
                        console.log('User dismissed the install prompt');
                    }
                    deferredPrompt = null;
                });
        }
    });
}

