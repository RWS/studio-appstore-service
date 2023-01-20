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

function SavePlugin(saveAs, saveStatusOnly) {
    var button = event.currentTarget;
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));
        ToggleLoader(button);
        data.set("Status", saveAs);
        data.set("SaveStatusOnly", saveStatusOnly);

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                if (request.responseText.includes('div')) {
                    document.getElementById('statusMessageContainer').innerHTML = request.responseText;

                    let alert = document.querySelector('.alert')
                    if (alert) {
                        setTimeout(() => {

                            alert.classList.add('slide-right');
                            alert.addEventListener('animationend', () => {
                                document.querySelector('.alert-container').remove();
                            })
                        }, 3000);
                    }

                    ToggleLoader(button);
                }
                else {
                    window.location.href = request.responseText;
                }
            }
        }

        request.open("POST", `/Plugins/Plugins/Save`);
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

function AddComment(pluginId) {
    let content = event.currentTarget.parentElement;

    $.ajax({
        type: "POST",
        url: `/Plugins/Edit/${pluginId}/Comments/New`,
        success: function (response) {
            content.innerHTML = response;
            Init();
        }
    });
}

function SaveComment(pluginId) {
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));
    let button = event.currentTarget;
    ToggleLoader(button);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE) {
            if (request.status == 200) {
                DiscardCommentEdit();
            }
            else {
                ToggleLoader(button);
            }
        }
    }

    request.open("POST", `/Plugins/Edit/${pluginId}/Comments/Update`);
    request.send(data);
}

function EditComment(commentId) {
    let url = new URL(window.location.href);

    url.searchParams.set("selectedComment", commentId);
    window.location.href = url.href;
}

function DeleteComment(pluginId, commentId) {
    document.getElementById("confirmationBtn").addEventListener('click', () => {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                window.location.reload();
            }
        }

        request.open("POST", `/Plugins/Edit/${pluginId}/Comments/Delete/${commentId}`);
        request.send(data);
    })
}
