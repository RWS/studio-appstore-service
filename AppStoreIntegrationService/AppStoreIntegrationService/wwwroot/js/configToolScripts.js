function AddNewVersion() {
    var pageValues = $('#editFile').find('select, textarea, input').serialize();
    var url = "Edit?handler=AddVersion";

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

function RedirectTo(goToPage, currentPage) {
    if (currentPage !== "/edit" && currentPage !== "/add") {
        window.location.href = `${goToPage}`;
        return;
    }

    var pageValues = "";
    var url = "";
    if (currentPage == "/add") {
        url = `Add?handler=GoToPage&pageUrl=${goToPage}`
        pageValues = $('#addPlugin').find('select, textarea, input').serialize();
    }

    if (currentPage == "/edit") {
        url = `Edit?handler=GoToPage&pageUrl=${goToPage}`
        pageValues = $('#editFile').find('select, textarea, input').serialize();
    }

    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: url,
        success: function (modalPartialView) {
            if (modalPartialView.includes("DOCTYPE")) {
                window.location.href = goToPage;
            }
            else {
                placeholderElement.html(modalPartialView);
                placeholderElement.find('.modal').modal('show');
            }
        }
    })
}

function DiscardChanges(goToPage) {
    window.location.href = `${goToPage}`;
}

function ShowVersionDetails(versionId, pageId) {
    var pageValues = "";
    var pageName = location.pathname.split("/").slice(-1).toString().toLowerCase();
    var url = `${pageName}?handler=ShowVersionDetails`;

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

let currentParagraphValue;
let oldData = '';
let newData = '';

function LoadSettingsPage() {
    let erasers = document.querySelectorAll('.table-row .name-mapping-eraser');
    let paragraphs = document.querySelectorAll('.name-mapping');
    let inputs = document.querySelectorAll('.name-mapping-input');
    let editers = document.querySelectorAll('.icon-cell .fa-pen-alt');
    let checkMarks = document.querySelectorAll('.icon-cell .fa-check-circle');
    let discardBtns = document.querySelectorAll('.icon-cell .custom-cross');
    let confirmDiscardBtn = document.getElementById('confirmDiscardChangesButton');
    let discardChangesBtn = document.getElementById('discardNameMappingChangesButton');

    discardBtns.forEach(btn => {
        RegisterEvent(btn, '#confirmDiscardNameMapping', confirmDiscardBtn);
    })

    checkMarks.forEach(mark => {
        RegisterEvent(mark, '#confirmEditNameMapping', discardChangesBtn);
    });

    editers.forEach(editer => {
        editer.addEventListener('click', (e) => {
            var inputs = editer.parentElement.parentElement.querySelectorAll('.editable-field .name-mapping-input');
            var paragraphs = editer.parentElement.parentElement.querySelectorAll('.editable-field .name-mapping');
            var editIcons = editer.parentElement.parentElement.querySelectorAll('.icon-cell i');

            CloseNewNameMappingForm(function () {
                CloseExistingEditForms(function () {
                    ToggleNameMappingEditForm(inputs, paragraphs, false, true);
                    UpdateEditPanelIcons(editIcons);
                });
            });

            e.stopImmediatePropagation();
        })
    })

    paragraphs.forEach(p => {
        p.addEventListener('dblclick', (e) => {
            e.target.classList.add('d-none');
            var input = e.target.parentElement.children[0];
            input.hidden = false;
            input.focus();
            currentParagraphValue = p.innerHTML;
            e.stopImmediatePropagation();
        })
    })

    inputs.forEach(input => {
        input.addEventListener('focusout', InputFocusEventListener)
    })

    erasers.forEach(eraser => {
        eraser.addEventListener('click', (e) => {
            var deleteNameMappingButton = document.getElementById('deleteNameMappingButton');
            CloseNewNameMappingForm();

            deleteNameMappingButton.onclick = function () {
                var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
                $.ajax({
                    data: pageValues,
                    type: "POST",
                    url: `PluginsRename?handler=DeleteNameMapping&Id=${eraser.id}`,
                    success: function () {
                        location.reload();
                    }
                })
            }

            $('#deleteNameMappingModal').modal('show');
            e.stopImmediatePropagation();
        })
    })
}

function CloseExistingEditForms(toggleCallback) {
    var openInputs = document.querySelectorAll('.name-mapping-input-open');
    var hiddenParagraphs = document.querySelectorAll('.name-mapping-closed');
    var editCells = [];
    newData = GetCurrentMappingData(openInputs);

    if (openInputs.length > 0) {
        editCells = openInputs[0].parentElement.parentElement.querySelectorAll('.icon-cell i');
    }

    if (oldData == newData) {
        UpdateEditPanelIcons(editCells);
        ToggleNameMappingEditForm(openInputs, hiddenParagraphs, false, false);
        toggleCallback();
    } else {
        document.getElementById('confirmDiscardChangesButton').onclick = function () {
            UpdateEditPanelIcons(editCells);
            ToggleNameMappingEditForm(openInputs, hiddenParagraphs, true, false);
            toggleCallback();
        }

        $('#confirmDiscardNameMapping').modal('show');
    }
}

function GetCurrentMappingData(inputs) {
    var data = '';
    for (let input of inputs) {
        data += input.value;
    }

    return data;
}

function ToggleNameMappingEditForm(inputs, paragraphs, restoreChanges, isOpen) {
    oldData = '';
    for (let j = 0; j < inputs.length; j++) {
        if (restoreChanges) {
            inputs[j].value = paragraphs[j].innerHTML;
        }

        inputs[j].classList.remove('name-mapping-input-open');
        paragraphs[j].classList.remove('name-mapping-closed');

        if (isOpen) {
            inputs[j].hidden = false;
            inputs[j].classList.add('name-mapping-input-open');
            inputs[j].removeEventListener('focusout', InputFocusEventListener);
            paragraphs[j].classList.add('name-mapping-closed');
            oldData += paragraphs[j].innerHTML;
        } else {
            inputs[j].hidden = true;
            inputs[j].addEventListener('focusout', InputFocusEventListener);
        }

        paragraphs[j].classList.toggle('d-none');
    }
}

function UpdateEditPanelIcons(editIcons) {
    for (let cell of editIcons) {
        cell.classList.toggle('d-none');
    }
}

function RegisterEvent(element, modalId, discardBtn) {
    element.addEventListener('click', (e) => {
        var inputs = element.parentElement.parentElement.querySelectorAll('.editable-field .name-mapping-input');
        var paragraphs = element.parentElement.parentElement.querySelectorAll('.editable-field .name-mapping');
        var editIcons = element.parentElement.parentElement.querySelectorAll('.icon-cell i');
        newData = GetCurrentMappingData(inputs);

        if (oldData == newData) {
            ToggleNameMappingEditForm(inputs, paragraphs, false, false);
            UpdateEditPanelIcons(editIcons);
        }
        else {
            discardBtn.onclick = function () {
                ToggleNameMappingEditForm(inputs, paragraphs, true, false);
                UpdateEditPanelIcons(editIcons);
            }

            $(modalId).modal('show');
        }

        oldData = '';
        e.stopImmediatePropagation();
    })
}

function InputFocusEventListener(e) {
    {
        if (currentParagraphValue != e.target.value) {
            UpdateNamesMapping();
        }

        e.target.hidden = true;
        e.target.parentElement.children[1].classList.remove('d-none');
        e.stopImmediatePropagation();
    }
}

function UpdateNamesMapping() {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=UpdateNamesMapping`,
        success: function (modalPartialView) {
            if (modalPartialView.includes("DOCTYPE")) {
                location.reload();
            }
            else {
                placeholderElement.html(modalPartialView);
                placeholderElement.find('.modal').modal('show');
            }
        }
    })
}

function AddNewNameMapping() {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=AddNewNameMapping`,
        success: function (partialView) {
            $("#newNameMappingPartial").html(partialView);
            CloseExistingEditForms(function () { return; })
            window.scrollTo(0, document.getElementById("newNameMappingPartial").getBoundingClientRect().y);
        }
    })
}

function CloseNewNameMappingForm(confirmationCallback) {
    var newNameMappingForm = document.getElementById("newNameMappingRow");
    if (newNameMappingForm != null) {
        var inputs = newNameMappingForm.querySelectorAll('.editable-field input');
        var data = GetCurrentMappingData(inputs);
        if (data == '') {
            newNameMappingForm.remove();
            confirmationCallback();

        } else {
            document.getElementById('confirmDiscardChangesButton').onclick = function () {
                newNameMappingForm.remove();
                confirmationCallback();
            }

            $('#confirmDiscardNameMapping').modal('show');
        }
    }

    confirmationCallback();
}
function AddNameMapping() {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=AddNameMapping`,
        success: function (modalPartialView) {
            if (modalPartialView.includes("DOCTYPE")) {
                location.reload();
            }
            else {
                placeholderElement.html(modalPartialView);
                placeholderElement.find('.modal').modal('show');
            }
        }
    })
}


function DeleteNameMapping(id) {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=DeleteNameMapping&Id=${id}`,
        success: function () {
            location.reload();
        }
    })
}


function ImportFile() {
    var formData = new FormData(document.getElementById("import-file-form"));
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: formData,
        type: "POST",
        contentType: false,
        processData: false,
        url: "ImportPlugins?handler=ImportFile",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    });
}


