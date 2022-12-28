function ImportPlugins() {
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                AjaxSuccessCallback(request.responseText);
                document.getElementById("modalContainer").innerHTML = request.responseText;
                $('#modalContainer').find('.modal').modal('show');
            }
        }

        request.open("POST", `ImportExportPlugins/CreateImport`);
        request.send(data);
    }
}