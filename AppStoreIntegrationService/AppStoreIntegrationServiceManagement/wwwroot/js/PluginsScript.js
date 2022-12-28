function DeletePlugin(id) {
    document.getElementById('confirmationBtn').onclick = function () {
        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                AjaxSuccessCallback(request.responseText);
                document.getElementById("modalContainer").innerHTML = request.responseText;
                $('#modalContainer').find('.modal').modal('show');
            }
        }

        request.open("POST", `Plugins/Plugins/Delete/${id}`);
        request.send();
    }
}