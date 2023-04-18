document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById("editor")) {
        editor = new Quill('#editor', {
            modules:
            {
                toolbar: quillToolbarOptions,
            },
            theme: 'snow'
        });
        isReadOnly = false;
        InitDropDown();
    }
})

function InitDropDown() {
    new DropDown(
        document.querySelector("#categoriesDropdown #dropDownToggle"),
        document.querySelector("#categoriesDropdown #CategoriesSelect"),
        $("#categoriesDropdown #Categories"),
        document.querySelector("#categoriesDropdown .selection-summary"),
        document.querySelectorAll("#categoriesDropdown .overflow-arrow"),
        [],
        isReadOnly
    ).Init();

    $.validator.setDefaults({
        ignore: '.ignore'
    });
}

function ClearLogs(id) {
    document.getElementById('confirmationBtn').onclick = function () {
        var data = new FormData();
        data.set("Id", id);
        SendPostRequest('/Plugins/Logs/ClearAll', data)
    }
}

function SavePlugin(action, removeOtherVersions = false) {
    let description = document.querySelector("#editor .ql-editor").innerHTML;
    document.getElementById("Description").innerText = description;

    $("#form").validate();

    if ($("#form").valid()) {
        let data = new FormData(document.getElementById("form"));
        data.set("RemoveOtherVersions", removeOtherVersions);
        SendPostRequest(`/Plugins/Plugins/${action}`, data);
    }
}

function Delete(action, id, needsConfirmation = true) {
    if (needsConfirmation) {
        document.getElementById('confirmationBtn').onclick = function () {
            SendPostRequest(`Plugins/Plugins/${action}/${id}`)
        }

        return;
    }

    SendPostRequest(`Plugins/Plugins/${action}/${id}`)
}