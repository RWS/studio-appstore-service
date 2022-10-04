function DeletePlugin(id, pluginName) {
    document.getElementById('confirmationBtn').onclick = function () {
        $.ajax({
            type: "POST",
            url: `Plugins/Plugins/Delete/${id}`,
            success: function () {
                location.reload();
            }
        })
    }

    document.querySelector("#confirmationModal .modal-body").innerHTML = `Are you sure you want to delete ${pluginName}?`;
    $('#confirmationModal').modal('show');
}