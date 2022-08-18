function RedirectTo(goToPage, currentPage) {
    if (currentPage == "/Add") {
        CreateRequest(goToPage, $('#addPlugin').find('select, textarea, input').serialize(), `Add?handler=GoToPage&pageUrl=${goToPage}`);
        return;
    }

    if (currentPage == "/Edit") {
        CreateRequest(goToPage, $('#editFile').find('select, textarea, input').serialize(), `Edit?handler=GoToPage&pageUrl=${goToPage}`);
        return;
    }

    if (currentPage.includes("PluginsRename")) {
        CreateRequest(goToPage, $('#namesMapping').find('select, textarea, input').serialize(), `/PluginsRename/GoToPage/${goToPage.replaceAll('/', '.')}`);
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