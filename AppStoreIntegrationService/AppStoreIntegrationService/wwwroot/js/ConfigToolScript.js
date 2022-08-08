function ShowNewPluginModal() {
    var placeholderElement = $('#addModalContainer');

    $.ajax({
        type: "GET",
        url: "ConfigTool?handler=AddPlugin",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function ShowConfirmationModal(id, name) {
    document.getElementById("selectedPluginId").value = id;
    document.getElementById("selectedPluginName").value = name;

    pageValues = $('#configToolPage').find('input').serialize();
    $.ajax({
        data: pageValues,
        type: "GET",
        url: "ConfigTool?handler=ShowDeleteModal",
        success: AjaxSuccessCallback
    })
}

function AjaxSuccessCallback(modalPartialView) {
    if (modalPartialView.includes("DOCTYPE")) {
        location.reload();
    }
    else {
        $('#modalContainer').html(modalPartialView);
        $('#modalContainer').find('.modal').modal('show');
    }
}