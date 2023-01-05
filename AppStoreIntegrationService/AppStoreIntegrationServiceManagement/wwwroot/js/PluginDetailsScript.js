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

function SavePlugin(saveAs) {
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));

        if (saveAs != undefined) {
            data.set("Status", saveAs);
        }

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                window.location.reload();
            }
        }

        request.open("POST", `/Plugins/Plugins/Save`);
        request.send(data);
    }
}