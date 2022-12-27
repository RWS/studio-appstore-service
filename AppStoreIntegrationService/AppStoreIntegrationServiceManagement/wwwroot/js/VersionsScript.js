let parentProducts;

function ShowVersion(pluginId, versionId) {
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
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Versions/${versionId}`);
    request.send();
}

function ToggleCaret(caret) {
    caret.classList.toggle("fa-caret-down");
    caret.classList.toggle("fa-caret-up");
}