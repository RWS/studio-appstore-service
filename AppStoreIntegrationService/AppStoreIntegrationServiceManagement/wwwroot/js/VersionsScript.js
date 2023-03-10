function InitDropDown() {
    document.addEventListener("DOMContentLoaded", () => {
        new DropDown(
            document.querySelector("#productsDropdown #dropDownToggle"),
            document.querySelector("#productsDropdown #ProductsSelect"),
            $("#productsDropdown #SupportedProducts"),
            document.querySelector("#productsDropdown .selection-summary"),
            document.querySelectorAll("#productsDropdown .overflow-arrow"),
            parentProducts.map(p => p.parentProductName),
            isReadOnly
        ).Init()

        $.validator.setDefaults({
            ignore: '.ignore'
        });
    })
}

function AddComment(pluginId, versionId) {
    let content = event.currentTarget.parentElement;
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            content.innerHTML = request.responseText;
            Init();
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Comments/${versionId}/New`);
    request.send();
}

function SaveComment(pluginId, versionId) {
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));
    let button = event.currentTarget;
    ToggleLoader(button);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            DiscardCommentEdit();
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Comments/${versionId}/Update`);
    request.send(data);
}

function DeleteComment(pluginId, versionId, commentId) {
    document.getElementById("confirmationBtn").addEventListener('click', () => {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                window.location.reload();
            }
        }

        request.open("POST", `/Plugins/Edit/${pluginId}/Comments/${versionId}/Delete/${commentId}`);
        request.send(data);
    })
}

function GenerateChecksum() {
    let request = new XMLHttpRequest();
    let spinner = document.getElementById("FileHash").parentElement.querySelector(".spinner-border");
    let data = new FormData(document.getElementById("form"));
    spinner.hidden = false;
    document.getElementById("FileHash").disabled = true;

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText);
            document.getElementById("FileHash").value = document.getElementById("GeneratedFileHash").value;
            document.getElementById("FileHash").disabled = false;
            spinner.hidden = true;
        }
    }

    request.open("POST", `/Plugins/Version/GenerateChecksum`);
    request.send(data);
}

function Save(pluginId, action, removeOtherVersions = false) {
    let button = event.currentTarget;

    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));
        data.set("RemoveOtherVersions", removeOtherVersions)
        ToggleLoader(button);

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                HttpRequestCallback(request.responseText);
                ToggleLoader(button);
            }
        }

        request.open("POST", `/Plugins/Edit/${pluginId}/Versions/${action}`);
        request.send(data);
    }
}

function ToggleLoader(element) {
    if (element.disabled) {
        element.disabled = false;
        element.firstElementChild.hidden = true;
        return;
    }

    element.disabled = true;
    element.firstElementChild.hidden = false;
}

function Delete(pluginId, versionId, action, needsConfirmation = true) {
    if (needsConfirmation) {
        document.getElementById('confirmationBtn').onclick = function () {
            RespondDeletionRequest(pluginId, versionId, action)
        }

        return;
    }

    RespondDeletionRequest(pluginId, versionId, action);
}

function RespondDeletionRequest(pluginId, versionId, action) {
    let request = new XMLHttpRequest();
    var data = new FormData();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            window.location.href = request.responseText;
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/${action}/${versionId}`);
    request.send(data);
}

function HttpRequestCallback(response) {
    if (!response.includes("div")) {
        window.location.href = response;
    }

    let alert = document.querySelector('.alert');

    if (alert) {
        alert.remove();
    }

    document.getElementById("statusMessageContainer").innerHTML = response;
    setTimeout(() => {
        alert = document.querySelector('.alert')
        alert.classList.add('slide-right');
        alert.addEventListener('animationend', () => {
            document.querySelector('.alert-container').remove();
        })
    }, 3000);
}