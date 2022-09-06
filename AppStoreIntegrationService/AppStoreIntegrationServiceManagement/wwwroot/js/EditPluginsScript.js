document.addEventListener('DOMContentLoaded', function () {
    var deleteBtns = document.querySelectorAll('#selectedVersion .fa-trash-alt');
    deleteBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.getElementById('confirmDeletePluginVersionButton').onclick = function () {
                var pageValues = $('#editFile').find('select, textarea, input').serialize();

                $.ajax({
                    async: true,
                    data: pageValues,
                    type: "POST",
                    url: `/Plugins/Version/Delete/${btn.id}`,
                    success: AjaxSuccessCallback
                })
            }

            $('#confirmDeletePluginVersion').modal('show');
        })
    })
});

function AddNewVersion() {
    var pageValues = $('#editFile').find('select, textarea, input').serialize();

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

function SavePlugin() {
    var pageValues = $('#editFile').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "/Plugins/Plugins/Update",
        success: AjaxSuccessCallback
    })
}

function ShowVersionDetails(versionId) {
    document.getElementById("selectedVersionId").value = versionId;
    var pageValues = $('#editFile').find('select, textarea, input').serialize();;

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "/Plugins/Version/Show",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
        }
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