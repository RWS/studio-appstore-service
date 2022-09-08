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
    var pageValues = $('#pluginDetails').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "/Plugins/Plugins/Create",
        success: AjaxSuccessCallback
    })
}

function AddNewVersion() {
    var pageValues = $('#pluginDetails').find('select, textarea, input').serialize();

    $.ajax({
        async: true,
        data: pageValues,
        type: "POST",
        url: "/Plugins/Version/Add",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
        }
    })
}