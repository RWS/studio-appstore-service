function ImportFile() {
    var formData = new FormData(document.getElementById("import-file-form"));

    $.ajax({
        data: formData,
        type: "POST",
        contentType: false,
        processData: false,
        url: "ImportPlugins?handler=ImportFile",
        success: AjaxSuccessCallback
    });
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