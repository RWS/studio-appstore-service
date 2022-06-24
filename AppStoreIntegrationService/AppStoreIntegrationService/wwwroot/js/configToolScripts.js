
function AddNewVersion(page) {
    var pageValues = "";
    var url = "";
    if (page == "edit") {
        pageValues = $('#editFile').find('select, textarea, input').serialize();
        url = "Edit?handler=AddVersion";
    }
    if (page == "add") {
        pageValues = $('#addPlugin').find('select, textarea, input').serialize();
        url = "Add?handler=AddVersion";
    }

    $.ajax({
        async: true,
        data: pageValues,
        type: "POST",
        url: url,
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
        }
    })
}

function SavePlugin() {
    var pageValues = $('#editFile').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "Edit?handler=SavePlugin",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function AddPlugin() {
    pageValues = $('#addPlugin').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "Add?handler=SaveVersionForPlugin",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function ShowNewPluginModal() {
    var placeholderElement = $('#addModalContainer');

    $.ajax({
        type: "GET",
        url: "ConfigTool?handler=AddPlugin",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function ShowConfirmationModal(id, name) {
    var placeholderElement = $('#modalContainer');
    document.getElementById("selectedPluginId").value = id;
    document.getElementById("selectedPluginName").value = name;

    pageValues = $('#configToolPage').find('input').serialize();
    console.log(pageValues);
    $.ajax({
        data: pageValues,
        type: "GET",
        url: "ConfigTool?handler=ShowDeleteModal",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function RedirectToPlugin(id) {
    window.location.href = `Edit?Id=${id}`;
}

function ReloadPage() {
    location.reload();
}

function RedirectToList(page) {
    var pageValues = "";
    var url = "";
    if (page == "add") {
        url = "Add?handler=BackToList";
        pageValues = $('#addPlugin').find('select, textarea, input').serialize();
    }

    if (page == "edit") {
        url = "Edit?handler=BackToList"
        pageValues = $('#editFile').find('select, textarea, input').serialize();
    }

    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: url,
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

function DiscardChanges() {
    window.location.href = "ConfigTool";
}

function ShowVersionDetails(versionId) {
    var pageValues = "";
    var pageName = location.pathname.split("/").slice(-1).toString().toLowerCase();
    var url = `${pageName}?handler=ShowVersionDetails`;

    console.log(pageName);
    document.getElementById("selectedVersionId").value = versionId;

    if (pageName == "edit") {
        pageValues = $('#editFile').find('select, textarea, input').serialize();
    }
    if (pageName == "add") {
        pageValues = $('#addPlugin').find('select, textarea, input').serialize();
    }

    $.ajax({
        async: true,
        data: pageValues,
        cache: false,
        type: "POST",
        url: url,
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
        }
    })
}

function DeleteVersion(id) {
    var pageValues = $('#editFile').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        async: true,
        data: pageValues,
        type: "POST",
        url: `Edit?handler=DeleteVersion&Id=${id}`,
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    })
}

