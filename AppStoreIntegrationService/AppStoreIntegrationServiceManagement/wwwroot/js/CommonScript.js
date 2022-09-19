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
        CreateRequest($('main').find('input').serialize(), `/Products/GoToPage/${goToPage}`);
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

document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
});