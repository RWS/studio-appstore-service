function RedirectTo(goToPage, currentPage) {
    if (currentPage == "Plugins/New") {
        CreateRequest(goToPage, $('#addPlugin').find('select, textarea, input').serialize(), `/Plugins/GoToPage/${goToPage.replaceAll('/', '.')}/new`);
        return;
    }

    if (currentPage == "Plugins/Edit") {
        CreateRequest(goToPage, $('#editFile').find('select, textarea, input').serialize(), `/Plugins/GoToPage/${goToPage.replaceAll('/', '.')}/edit`);
        return;
    }

    if (currentPage == "PluginsRename/Index") {
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