﻿function AddPlugin() {
    $("#form").validate();

    if ($("#form").valid()) {
        var pageValues = $('main').find('select, textarea, input').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: "/Plugins/Plugins/Create",
            success: function (actionResult) {
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
        })
    }
}

function AddNewVersion() {
    $.ajax({
        type: "POST",
        url: "/Plugins/Version/Add",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
            $('#form').data('validator', null);
            $.validator.unobtrusive.parse('#form');
        }
    })
}