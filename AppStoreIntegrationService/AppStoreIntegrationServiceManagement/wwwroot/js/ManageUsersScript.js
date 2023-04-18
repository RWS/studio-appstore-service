function ConsentAgreement() {
    let data = new FormData(document.getElementById("form"));
    SendPostRequest('/Identity/Authentication/ConsentAgreement', data)
}

function DismissUser(userId, isCurrentUser = false, accountId = '') {
    let modal = document.getElementById("confirmationModal");

    if (isCurrentUser) {
        modal.querySelector(".modal-body").innerHTML = "Are you sure you want to leave this account?"
    }

    document.getElementById('confirmationBtn').onclick = function () {
        let data = new FormData();

        data.set("UserId", userId);
        data.set("AccountId", accountId);
        SendPostRequest('/Identity/Account/Dismiss',data)
    }
}

function HideUserDetails() {
    let userInfo = event.currentTarget.parentElement.parentElement.nextElementSibling;
    let feedbackSpan = document.getElementById("userCheckMessage");

    if (userInfo.classList.contains("d-none")) {
        return;
    }

    if (!feedbackSpan.classList.contains("d-none")) {
        feedbackSpan.classList.add("d-none");
    }

    userInfo.classList.add("d-none");
}

function Register() {
    let data = new FormData(document.getElementById("form"));
    SendPostRequest('/Identity/Account/PostRegister', data)
}

function Login() {
    let data = new FormData(document.getElementById("form"));
    SendPostRequest('/Identity/Authentication/PostLogin', data)
}

function CheckUserExistance() {
    $("#Email").validate();

    if ($("#Email").valid()) {
        let button = event.target;
        let data = new FormData(document.getElementById("form"));
        let feedbackElement = document.getElementById("userCheckMessage");
        let userInfo = button.parentElement.parentElement.nextElementSibling;
        let request = new XMLHttpRequest();

        ToggleLoader(button)

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                if (request.responseText.includes("div")) {
                    HttpRequestCallback(request.responseText, true);
                    feedbackElement.innerText = null;
                } else {
                    var response = JSON.parse(request.responseText);
                    feedbackElement.className = null;
                    feedbackElement.innerText = response.message;
                    feedbackElement.classList.add(response.isErrorMessage ? "text-danger" : "text-success");
                    userInfo.classList.remove(response.isErrorMessage ? null : "d-none");
                }

                ToggleLoader(button)
            }
        }

        request.open("POST", `/Identity/Account/CheckUserExistance`);
        request.send(data);
    }
}

function AddUserToAccount() {
    let data = new FormData(document.getElementById("assignmentForm"));
    SendPostRequest('/Identity/Account/Assign', data)
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