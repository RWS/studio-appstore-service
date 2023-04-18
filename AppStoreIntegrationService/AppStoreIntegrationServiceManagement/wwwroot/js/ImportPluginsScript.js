function ImportPlugins() {
    $("#form").validate();

    if ($("#form").valid()) {
        let data = new FormData(document.getElementById("form"));
        SendPostRequest('ImportExportPlugins/CreateImport', data, true)
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