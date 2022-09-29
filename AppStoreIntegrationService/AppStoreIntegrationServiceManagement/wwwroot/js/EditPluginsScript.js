document.addEventListener('DOMContentLoaded', function () {
    var deleteBtns = document.querySelectorAll('#selectedVersion .fa-trash-alt');
    deleteBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.getElementById('confirmationBtn').onclick = function () {
                var pageValues = $('main').find('select, textarea, input').serialize();

                $.ajax({
                    data: pageValues,
                    type: "POST",
                    url: `/Plugins/Version/Delete/${btn.id}`,
                    success: function () {
                        location.reload();
                    }
                })
            }

            document.querySelector("#confirmationModal .modal-body").innerHTML = "Are you sure you want to delete this plugin version?";
            $('#confirmationModal').modal('show');
        })
    })
});

function SavePlugin() {
    $("#form").validate();

    if ($("#form").valid()) {
        var pageValues = $('main').find('select, textarea, input').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: "/Plugins/Plugins/Update",
            success: AjaxSuccessCallback
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

function ShowVersionDetails(versionId) {
    document.getElementById("selectedVersionId").value = versionId;
    var pageValues = $('main').find('select, textarea, input').serialize();;

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "/Plugins/Version/Show",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
            $('#form').data('validator', null);
            $.validator.unobtrusive.parse('#form');
        }
    })
}

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.href = actionResult;
        return;
    }

    $('.alert').remove();
    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}