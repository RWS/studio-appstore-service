let parentProducts;

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
    document.getElementById("CommentDescription").value = document.querySelector('.edit-area').innerHTML
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            window.location.reload();
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
                window.location.reload();
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