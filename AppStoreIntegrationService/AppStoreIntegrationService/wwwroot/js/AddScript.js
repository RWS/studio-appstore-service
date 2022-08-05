function AjaxSuccessCallback(modalPartialView) {
    if (modalPartialView.includes("DOCTYPE")) {
        location.reload();
    }
    else {
        $('#modalContainer').html(modalPartialView);
        $('#modalContainer').find('.modal').modal('show');
    }
}

function AddPlugin() {
    pageValues = $('#addPlugin').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "Add?handler=SaveVersionForPlugin",
        success: AjaxSuccessCallback
    })
}

function RedirectTo(goToPage) {
    var pageValues = $('#addPlugin').find('select, textarea, input').serialize();;
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `Add?handler=GoToPage&pageUrl=${goToPage}`,
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

function ShowVersionDetails(versionId) {
    document.getElementById("selectedVersionId").value = versionId;
    var pageValues = $('#addPlugin').find('select, textarea, input').serialize();;

    $.ajax({
        async: true,
        data: pageValues,
        cache: false,
        type: "POST",
        url: "Add?handler=ShowVersionDetails",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
        }
    })
}