document.addEventListener("DOMContentLoaded", () => {
    $("#form").data('validator', null);
    $.validator.unobtrusive.parse("#form");

    if (new URL(window.location.href).searchParams.get("selectedView") == "Details") {
        new DropDown(
            document.querySelector("#productsDropdown #dropDownToggle"),
            document.querySelector("#productsDropdown #ProductsSelect"),
            $("#productsDropdown #SupportedProducts"),
            document.querySelector("#productsDropdown .selection-summary"),
            document.querySelectorAll("#productsDropdown .overflow-arrow"),
            parentProducts.map(p => p.parentProductName)
        ).Init()
    }
})

function Show(versionId, view) {
    let url = new URL(window.location.href);

    url.searchParams.set("selectedVersion", versionId);
    url.searchParams.set("selectedView", view);
    window.location.href = url.href;
}

function AddComment(pluginId, versionId) {
    let content = event.currentTarget.parentElement;

    $.ajax({
        type: "POST",
        url: `/Plugins/Edit/${pluginId}/Comments/${versionId}/New`,
        success: function (response) {
            content.innerHTML = response;
            Init();
        }
    });
}

function SaveComment(pluginId, versionId) {
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));
    let button = event.currentTarget;
    ToggleLoader(button);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE) {
            if (request.status == 200) {
                DiscardCommentEdit();
            } else {
                ToggleLoader(button);
            }
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
    let spinner = document.querySelector(".spinner-border");
    let data = new FormData(document.getElementById("form"));
    spinner.hidden = false;
    document.getElementById("FileHash").disabled = true;

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            AjaxSuccessCallback(request.responseText);
            document.getElementById("FileHash").value = document.getElementById("GeneratedFileHash").value;
            document.getElementById("FileHash").disabled = false;
            spinner.hidden = true;
        }
    }

    request.open("POST", `/Plugins/Version/GenerateChecksum`);
    request.send(data);
}

function SaveVersion(pluginId, saveAs, saveStatusOnly) {
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));
    let button = event.currentTarget;
    ToggleLoader(button);
    data.set("VersionStatus", saveAs);
    data.set("SaveStatusOnly", saveStatusOnly);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            AjaxSuccessCallback(request.responseText);
            ToggleLoader(button);
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/Save`);
    request.send(data);
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

function DeleteVersion(pluginId, versionId, deletionApproval) {
    document.getElementById('confirmationBtn').onclick = function () {
        ApproveDeletion(pluginId, versionId, deletionApproval);
    }
}

function ApproveDeletion(pluginId, versionId, deletionApproval) {
    let request = new XMLHttpRequest();
    var data = new FormData();
    data.set("DeletionApproval", deletionApproval)

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            window.location.reload();
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/Delete/${versionId}`);
    request.send(data);
}

function UpdatePlaceholder() {
    let placeholder = event.target.closest('.version-details').previousElementSibling.querySelector(".version-number-placeholder");
    let text = event.target.value;
    if (text == '') {
        placeholder.innerText = "Version number"
        return;
    }

    placeholder.innerText = text;
}

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.reload();
        return;
    }

    document.querySelector('.alert').remove();
    document.getElementById("statusMessageContainer").innerHTML = actionResult;
    var timeout = new setTimeout(() => {
        let alert = document.querySelector('.alert')
        alert.classList.add('slide-up');
        alert.addEventListener('animationend', () => {
            alert.remove();
        })
    }, 3000);

    clearTimeout(timeout);
}