document.addEventListener('DOMContentLoaded', function () {
    new DropDown(
        document.querySelector("#categoriesDropdown #dropDownToggle"),
        document.querySelector("#categoriesDropdown #CategoriesSelect"),
        $("#categoriesDropdown #Categories"),
        document.querySelector("#categoriesDropdown .selection-summary"),
        document.querySelectorAll("#categoriesDropdown .overflow-arrow"),
        []
    ).Init();

    $.validator.setDefaults({
        ignore: '.ignore'
    });
});

function UpdateDescription() {
    document.getElementById("Description").value = event.target.innerHTML;
}

function SavePlugin() {
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                AjaxSuccessCallback(request.responseText);
                let comparison = JSON.parse(document.getElementById("ManifestPluginComparison").value)
                document.getElementById("PluginNameManifestConflict").hidden = comparison.isNameMatch;
                document.getElementById("DeveloperNameManifestConflict").hidden = comparison.isAuthorMatch;
            }
        }

        request.open("POST", `/Plugins/Plugins/Save`);
        request.send(data);
    }
}

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.href = actionResult;
        return;
    }

    $('.alert').remove();
    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}