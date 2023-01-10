function DeletePlugin(id, deletionApproved) {
    document.getElementById('confirmationBtn').onclick = function () {
        ApproveDeletion(id, deletionApproved);
    }
}

function ApproveDeletion(id, deletionApproved) {
    let request = new XMLHttpRequest();
    var data = new FormData();
    data.set("DeletionApproved", deletionApproved)

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            AjaxSuccessCallback(request.responseText);
            document.getElementById("modalContainer").innerHTML = request.responseText;
            $('#modalContainer').find('.modal').modal('show');
        }
    }

    request.open("POST", `Plugins/Plugins/Delete/${id}`);
    request.send(data);
}