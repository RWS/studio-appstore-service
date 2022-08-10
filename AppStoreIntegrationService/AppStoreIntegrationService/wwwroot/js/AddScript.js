﻿function AjaxSuccessCallback(modalPartialView) {
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