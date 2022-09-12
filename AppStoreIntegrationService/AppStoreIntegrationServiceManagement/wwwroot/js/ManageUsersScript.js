document.addEventListener('DOMContentLoaded', function () {
    const deleteBtns = document.querySelectorAll(".delete-user-btn");

    deleteBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.getElementById('confirmationBtn').onclick = function () {
                $.ajax({
                    type: "POST",
                    url: `/Identity/Account/Delete/${btn.id}`,
                    success: function () {
                        location.reload();
                    }
                })
            }
            
            document.querySelector("#confirmationModal .modal-body").innerHTML = "Are you sure you want to delete this user?";
            $('#confirmationModal').modal('show');
            e.stopImmediatePropagation();
        })
    })
});