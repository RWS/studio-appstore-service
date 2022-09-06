document.addEventListener('DOMContentLoaded', function () {
    var deletePluginBtns = document.querySelectorAll('#configToolPage .delete-plugin-btn');

    deletePluginBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            document.getElementById('confirmDeletePluginButton').onclick = function () {
                $.ajax({
                    data: "",
                    type: "POST",
                    url: `Plugins/Plugins/Delete/${btn.id}`,
                    success: AjaxSuccessCallback
                })
            }

            document.querySelector('.plugin-name-placeholder').innerHTML = document.querySelector('#configToolPage .plugin-name').innerHTML;
            $('#confirmDeletePlugin').modal('show');
        })
    })
});

function AjaxSuccessCallback(modalPartialView) {
    if (modalPartialView.includes("DOCTYPE")) {
        location.reload();
    }
    else {
        $('#modalContainer').html(modalPartialView);
        $('#modalContainer').find('.modal').modal('show');
    }
}