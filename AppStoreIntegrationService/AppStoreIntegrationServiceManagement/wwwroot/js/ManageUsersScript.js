window.onload = () => {
    var erasers = document.querySelectorAll(".delete-user-btn");

    erasers.forEach(eraser => {
        eraser.addEventListener('click', (e) => {
            document.getElementById('deleteUserButton').onclick = function () {
                $.ajax({
                    type: "POST",
                    url: `/Identity/Manage/Delete/${eraser.id}`,
                    success: function () {
                        location.reload();
                    }
                })
            }

            $('#deleteUserModal').modal('show');
            e.stopImmediatePropagation();
        })
    })
}