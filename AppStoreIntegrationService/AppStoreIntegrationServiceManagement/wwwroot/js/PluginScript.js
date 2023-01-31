﻿function InitDropDown() {
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

function SavePlugin(action, removeOtherVersions = false) {
    var button = event.currentTarget;
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));
        data.set("RemoveOtherVersions", removeOtherVersions);
        ToggleLoader(button);

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                HttpRequestCallback(request.responseText)
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

function HttpRequestCallback(response) {
    if (response.includes('div')) {
        document.getElementById('statusMessageContainer').innerHTML = response;

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
        window.location.href = response;
    }
}
