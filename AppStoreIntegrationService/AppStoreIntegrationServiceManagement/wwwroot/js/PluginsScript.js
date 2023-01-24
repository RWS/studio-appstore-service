function Delete(action, id) {
    document.getElementById('confirmationBtn').onclick = function () {
        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                HttpRequestCallback(request.responseText);
            }
        }

        request.open("POST", `Plugins/Plugins/${action}/${id}`);
        request.send();
    }
}

function HttpRequestCallback(response) {
    if (response.includes('div')) {
        document.getElementById('statusMessageContainer').innerHTML = response;

        let alert = document.querySelector('.alert')
        if (alert) {
            setTimeout(() => {

                alert.classList.add('slide-right');
                alert.addEventListener('animationend', () => {
                    document.querySelector('.alert-container').remove();
                })
            }, 3000);
        }

        ToggleLoader(button);
    }
    else {
        window.location.href = response;
    }
}