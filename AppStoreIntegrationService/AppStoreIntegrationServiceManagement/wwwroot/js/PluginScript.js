document.addEventListener('DOMContentLoaded', () => {
    var editorExists = document.getElementById("editor") != null;

    if (editorExists) {
        var toolbarOptions = [
            ['bold', 'italic', 'underline', 'strike'],
            ['blockquote', 'code-block'],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            [{ 'script': 'sub' }, { 'script': 'super' }],
            [{ 'indent': '-1' }, { 'indent': '+1' }],
            [{ 'align': [] }],
            ['clean'],
            [{ 'direction': 'rtl' }],
            [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
            [{ 'color': [] }, { 'background': [] }],
            ['link', 'image', 'video'],
        ];

        editor = new Quill('#editor', {
            modules:
            {
                toolbar: toolbarOptions,
            },
            theme: 'snow'
        });
        isReadOnly = false;
        InitDropDown();
    }
})

function InitDropDown() {
    document.addEventListener('DOMContentLoaded', function () {
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
    });
}

function ClearLogs(id) {
    document.getElementById('confirmationBtn').onclick = function () {
        var data = new FormData();
        data.set("Id", id);

        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                HttpRequestCallback(request.responseText);
            }
        }

        request.open("POST", `/Plugins/Logs/ClearAll`);
        request.send(data)
    }
}

function SavePlugin(action, removeOtherVersions = false) {
    var button = event.currentTarget;
    document.getElementById("Description").innerText = document.querySelector("#editor .ql-editor").innerHTML;
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));
        data.set("RemoveOtherVersions", removeOtherVersions);
        ToggleLoader(button);

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                HttpRequestCallback(request.responseText);
                ToggleLoader(button);
            }
        }

        request.open("POST", `/Plugins/Plugins/${action}`);
        request.send(data);
    }
}

function Delete(action, id, needsConfirmation = true) {
    if (needsConfirmation) {
        document.getElementById('confirmationBtn').onclick = function () {
            RespondDeletionRequest(action, id);
        }

        return;
    }

    RespondDeletionRequest(action, id);
}

function RespondDeletionRequest(action, id) {
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText);
        }
    }

    request.open("POST", `Plugins/Plugins/${action}/${id}`);
    request.send();
}
