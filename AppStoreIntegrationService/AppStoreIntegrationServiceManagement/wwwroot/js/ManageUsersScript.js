document.addEventListener('DOMContentLoaded', function () {
    const deleteBtns = document.querySelectorAll(".delete-user-btn");

    deleteBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.getElementById('deleteUserButton').onclick = function () {
                $.ajax({
                    type: "POST",
                    url: `/Identity/Manage/Delete/${btn.id}`,
                    success: function () {
                        location.reload();
                    }
                })
            }

            $('#deleteUserModal').modal('show');
            e.stopImmediatePropagation();
        })
    })
});