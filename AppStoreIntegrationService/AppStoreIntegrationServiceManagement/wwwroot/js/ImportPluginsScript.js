function ImportFile() {
    var formData = new FormData(document.getElementById("import-file-form"));

    $.ajax({
        data: formData,
        async: true,
        type: "POST",
        contentType: false,
        processData: false,
        url: "ImportExportPlugins/CreateImport",
        success: function (actionResult) {
            $('#modalContainer').html(actionResult);
            $('#modalContainer').find('.modal').modal('show');
        }
    });
}