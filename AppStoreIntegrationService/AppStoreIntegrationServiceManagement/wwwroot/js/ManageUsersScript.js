function DeleteUser(userId) {
    document.getElementById('confirmationBtn').onclick = function () {
        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                HttpRequestCallback(request.responseText);
            }
        }

        request.open("POST", `/Identity/Account/Delete/${userId}`);
        request.send();
    }
}

function ConsentAgreement() {
    let data = new FormData(document.getElementById("form"));
    let request = new XMLHttpRequest();
    data.set("AcceptedAgreement", data.get("Item1"))

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText)
        }
    }

    request.open("POST", `/Identity/Authentication/ConsentAgreement`);
    request.send(data);
}

function DismissUser(userId) {
    document.getElementById('confirmationBtn').onclick = function () {
        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                window.location.reload();
            }
        }

        request.open("POST", `/Identity/Account/Dismiss/${userId}`);
        request.send();
    }
}

function HideUserDetails() {
    let userInfo = event.currentTarget.parentElement.parentElement.nextElementSibling;
    let feedSpan = document.getElementById("userCheckMessage");

    if (userInfo.classList.contains("d-none")) {
        return;
    }

    if (!feedSpan.classList.contains("d-none")) {
        feedSpan.classList.add("d-none");
    }
    
    userInfo.classList.add("d-none");
}

function Register() {
    let data = new FormData(document.getElementById("form"));
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText)
        }
    }

    request.open("POST", `/Identity/Account/PostRegister`);
    request.send(data);
}

function Login() {
    let data = new FormData(document.getElementById("form"));
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText)
        }
    }

    request.open("POST", `/Identity/Authentication/PostLogin`);
    request.send(data);
}

function CheckUserExistance() {
    $("#Email").validate();

    if (!$("#Email").valid()) {
        return;
    }

    let button = event.target;
    let data = new FormData();
    let feedbackElement = document.getElementById("userCheckMessage");
    let info = button.parentElement.parentElement.nextElementSibling;
    data.set("Email", button.previousElementSibling.value);
    ToggleLoader(button)

    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            if (request.responseText.includes("div")) {
                document.getElementById("modalContainer").innerHTML = request.responseText;
                $('#modalContainer').find('.modal').modal('show');
                feedbackElement.innerText = null;
            } else {
                var response = JSON.parse(request.responseText);
                feedbackElement.className = null;
                feedbackElement.innerText = response.message;
                feedbackElement.classList.add(response.isErrorMessage ? "text-danger" : "text-success");
                info.classList.remove(response.isErrorMessage ? null : "d-none");
            }

            ToggleLoader(button)
        }
    }

    request.open("POST", `/Identity/Account/CheckUserExistance`);
    request.send(data);
}

function AddUserToAccount() {
    let data = new FormData(document.getElementById("assignmentForm"));
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText)
        }
    }

    request.open("POST", `/Identity/Account/Assign`);
    request.send(data);
}

function ChangeOwner() {
    let data = new FormData(document.getElementById("newOwnerForm"));
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText)
        }
    }

    request.open("POST", `/Identity/Account/ChangeOwner`);
    request.send(data);
}

function GenerateAccessToken() {
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            document.getElementById("accessTokenContainer").innerHTML = request.responseText;
        }
    }

    request.open("POST", `/Identity/Account/GenerateAccessToken`);
    request.send();
}

function ToggleAccessCheckbox() {
    let input = event.currentTarget;
    let accessInput = input.parentElement.nextElementSibling;

    if (input.checked) {
        accessInput.classList.remove("d-none");
        return;
    }

    if (!input.checked) {
        accessInput.classList.add("d-none");
        accessInput.firstElementChild.checked = false;
        return;
    }
}