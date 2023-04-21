document.addEventListener('DOMContentLoaded', () => {
    isReadOnly = false;

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
        let data = new FormData(document.getElementById("form"));
        SendPostRequest(`/Plugins/Edit/${pluginId}/Comments/${versionId}/Delete/${commentId}`, data)
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
    $("#form").validate();

    if ($("#form").valid()) {
        let data = new FormData(document.getElementById("form"));
        data.set("RemoveOtherVersions", removeOtherVersions);
        SendPostRequest(`/Plugins/Edit/${pluginId}/Versions/${action}`, data);
    }
}

function Delete(pluginId, versionId, action, needsConfirmation = true) {
    if (needsConfirmation) {
        document.getElementById('confirmationBtn').onclick = function () {
            SendPostRequest(`/Plugins/Edit/${pluginId}/Versions/${action}/${versionId}`);
        }

        return;
    }

    SendPostRequest(`/Plugins/Edit/${pluginId}/Versions/${action}/${versionId}`);
}