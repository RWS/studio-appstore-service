let parentProducts;

function Show(pluginId, versionId) {
    let content = event.currentTarget.parentElement.nextElementSibling;
    let caret = event.currentTarget.firstElementChild;
    let request = new XMLHttpRequest();
    let openVersion = document.querySelector('.version-details[aria-expanded="true"]');
    let caretUp = document.querySelector('.fa-caret-up');

    if (openVersion) {
        openVersion.innerHTML = "";
        openVersion.ariaExpanded = false;
        ToggleCaret(caretUp);

        if (openVersion == content) {
            return;
        }
    }

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            content.innerHTML = request.responseText;
            content.ariaExpanded = true;
            ToggleCaret(caret);
            PrepareForm();
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/${versionId}`);
    request.send();
}

function ShowDetails(pluginId, versionId) {
    let content = event.currentTarget.closest('.version-details');
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            content.innerHTML = request.responseText;
            PrepareForm();
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/${versionId}`);
    request.send();
}

function ShowComments(pluginId, versionId) {
    let content = event.currentTarget.closest('.version-details');
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            content.innerHTML = request.responseText;
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Comments/${versionId}`);
    request.send();
}

function GenerateChecksum() {
    let request = new XMLHttpRequest();
    let spinner = document.querySelector(".spinner-border");
    let data = new FormData(document.getElementById("form"));
    spinner.hidden = false;

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            AjaxSuccessCallback(request.responseText);
            document.getElementById("FileHash").value = document.getElementById("GeneratedFileHash").value;
            spinner.hidden = true;
        }
    }

    request.open("POST", `/Plugins/Version/GenerateChecksum`);
    request.send(data);
}

function SaveVersion(pluginId) {
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            AjaxSuccessCallback(request.responseText);
            let compare = JSON.parse(document.getElementById("ManifestVersionComparison").value)
            document.getElementById("VersionNumberManifestConflict").hidden = compare.isVersionMatch;
            document.getElementById("MinVersionManifestConflict").hidden = compare.isMinVersionMatch;
            document.getElementById("MaxVersionManifestConflict").hidden = compare.isMaxVersionMatch;
            document.getElementById("ProductManifestConflict").hidden = compare.isProductMatch;
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/Save`);
    request.send(data);
}

function DeleteVersion(pluginId, versionId) {
    document.getElementById('confirmationBtn').onclick = function () {
        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                AjaxSuccessCallback(request.responseText);
            }
        }

        request.open("POST", `/Plugins/Edit/${pluginId}/Versions/Delete/${versionId}`);
        request.send();
    }
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

function ToggleCaret(caret) {
    caret.classList.toggle("fa-caret-down");
    caret.classList.toggle("fa-caret-up");
}

function PrepareForm() {
    $("#form").data('validator', null);
    $.validator.unobtrusive.parse("#form");

    new DropDown(
        document.querySelector("#productsDropdown #dropDownToggle"),
        document.querySelector("#productsDropdown #ProductsSelect"),
        $("#productsDropdown #SupportedProducts"),
        document.querySelector("#productsDropdown .selection-summary"),
        document.querySelectorAll("#productsDropdown .overflow-arrow"),
        parentProducts.map(p => p.parentProductName)
    ).Init()
}

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.reload();
        return;
    }

    $('.alert').remove();
    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}