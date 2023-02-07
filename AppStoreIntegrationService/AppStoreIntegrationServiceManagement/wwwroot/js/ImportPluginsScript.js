function ImportPlugins() {
    $("#form").validate();

    if ($("#form").valid()) {
        let request = new XMLHttpRequest();
        let data = new FormData(document.getElementById("form"));

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                document.getElementById("modalContainer").innerHTML = request.responseText;
                $('#modalContainer').find('.modal').modal('show');
            }
        }

        request.open("POST", `ImportExportPlugins/CreateImport`);
        request.send(data);
    }
}

function SwitchNotifications() {
    var data = new FormData(document.getElementById("form"))
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            return;
        }
    }

    request.open("POST", `/Identity/Notifications/Update`);
    request.send(data);
}