function RedirectTo(goToPage, controller, action) {
    var currentPage = controller + '/' + action;

    if (currentPage == "Plugins/Edit" || currentPage == "Plugins/New") {
        CreateRequest(goToPage, $('#pluginDetails').find('select, textarea, input').serialize(), `/Plugins/GoToPage/${goToPage.replaceAll('/', '.')}/${action}`);
        return;
    }

    if (controller == "PluginsRename") {
        CreateRequest(goToPage, $('#namesMapping').find('input').serialize(), `/PluginsRename/GoToPage/${goToPage.replaceAll('/', '.')}`);
        return;
    }

    if (controller == "Account" && action != "Users") {
        CreateRequest(goToPage, $('body').find("input, input[type='radio']").serialize(), `/Account/GoToPage/${goToPage.replaceAll('/', '.')}/${action}`);
        return;
    }

    window.location.href = `${goToPage}`;
}

function CreateRequest(goToPage, pageValues, url) {
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: url,
        success: function (modalPartialView) {
            if (modalPartialView.includes("DOCTYPE")) {
                window.location.href = goToPage;
            }
            else {
                placeholderElement.html(modalPartialView);
                placeholderElement.find('.modal').modal('show');
            }
        }
    })
}

function DiscardChanges(goToPage) {
    window.location.href = `${goToPage}`;
}

document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    $(".alert").fadeTo(2000, 500).slideUp(500, function () {
        $(".alert").slideUp(500);
    });
});