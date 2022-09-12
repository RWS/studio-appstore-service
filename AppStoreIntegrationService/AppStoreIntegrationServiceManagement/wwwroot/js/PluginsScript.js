document.addEventListener('DOMContentLoaded', function () {
    var deletePluginBtns = document.querySelectorAll('#configToolPage .delete-plugin-btn');
    deletePluginBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.getElementById('confirmationBtn').onclick = function () {
                $.ajax({
                    type: "POST",
                    url: `Plugins/Plugins/Delete/${btn.id}`,
                    success: function() {
                        location.reload();
                    }
                })
            }

            var pluginName = document.querySelector('#configToolPage .plugin-name').innerHTML;
            document.querySelector("#confirmationModal .modal-body").innerHTML = `Are you sure you want to delete ${pluginName}?`;
            $('#confirmationModal').modal('show');

            e.stopImmediatePropagation();
        })
    })
});