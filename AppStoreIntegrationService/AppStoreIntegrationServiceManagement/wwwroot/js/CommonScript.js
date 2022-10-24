function RedirectTo(goToPage, controller, action) {
    var currentPage = controller + '/' + action;

    if (currentPage == "Plugins/Edit" || currentPage == "Plugins/New") {
        CreateRequest($('main').find('select, textarea, input').serialize(), `/Plugins/GoToPage/${goToPage}/${action}`);
        return;
    }

    if (controller == "PluginsRename") {
        CreateRequest($('main').find('input').serialize(), `/PluginsRename/GoToPage/${goToPage}`);
        return;
    }

    if (controller == "Products") {
        CreateRequest($('main').find('input, select').serialize(), `/Products/GoToPage/${goToPage}`);
        return;
    }

    if (controller == "Categories") {
        CreateRequest($('main').find('input, select').serialize(), `/Categories/GoToPage/${goToPage}`);
        return;
    }

    if (controller == "Account" && action != "Users") {
        CreateRequest($('main').find("input, input[type='radio']").serialize(), `/Account/GoToPage/${goToPage}/${action}`);
        return;
    }

    goToPage = goToPage.replaceAll('.', '\\');
    window.location.href = `${goToPage}`;
}

function CreateRequest(pageValues, url) {

    $.ajax({
        data: pageValues,
        type: "POST",
        url: url,
        success: function (actionResult) {
            if (!actionResult.includes("div")) {
                window.location.href = actionResult;
            }
            else {
                $('#modalContainer').html(actionResult);
                $('#modalContainer').find('.modal').modal('show');
            }
        }
    })
}

function Collapse(element) {

    if (window.innerWidth > 992) {
        return;
    }

    element.parentElement.classList.toggle('bg-collapsed');

    var children = element.closest('tr').children;
    for (let child of children) {
        if (child != element.parentElement) {
            child.classList.toggle('d-none');
        }
    }
}

function GenerateChecksum() {
    var pageValues = $('main').find('select, textarea, input').serialize();
    var button = event.target;
    $("#DownloadUrl").validate();

    if ($("#DownloadUrl").valid()) {
        button.disabled = true;
        button.firstElementChild.hidden = false;

        $.ajax({
            data: pageValues,
            type: "POST",
            url: "/Plugins/Version/GenerateChecksum",
            success: function (result) {
                button.disabled = false;
                button.firstElementChild.hidden = true;
                AjaxSuccessCallback(result);
                document.getElementById("FileHash").value = fileHash;
            }
        })
    }
}

function ManageChecksum(url, checksum) {
    var currentInput = event.target.value;

    if (url != currentInput) {
        document.getElementById("FileHash").value = "";
        return;

    }

    document.getElementById("FileHash").value = checksum;
}

document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
});

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.href = actionResult;
        return;
    }

    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}