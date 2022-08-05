let currentParagraphValue;
let oldData = '';
let newData = '';

window.onload = () => {

    const erasers = document.querySelectorAll('.table-row .name-mapping-eraser');
    const paragraphs = document.querySelectorAll('.name-mapping');
    const inputs = document.querySelectorAll('.name-mapping-input');
    const editers = document.querySelectorAll('.icon-cell .fa-pen-alt');
    const checkMarks = document.querySelectorAll('.icon-cell .fa-check-circle');
    const discardBtns = document.querySelectorAll('.icon-cell .custom-cross');

    discardBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            ToggleEditForm(GetCurrentRowElements(btn), function () { return; }, '#confirmDiscardNameMapping', document.getElementById('confirmDiscardChangesButton'));
            e.stopImmediatePropagation();
        })
    })

    checkMarks.forEach(mark => {
        mark.addEventListener('click', (e) => {
            ToggleEditForm(GetCurrentRowElements(mark), function () { return; }, '#confirmEditNameMapping', document.getElementById('discardNameMappingChangesButton'));
            e.stopImmediatePropagation();
        })
    });

    editers.forEach(editer => {
        editer.addEventListener('click', (e) => {
            const [inputs, paragraphs, icons] = GetCurrentRowElements(editer);

            CloseNewNameMappingForm(function () {
                CloseExistingEditForms(function () {
                    ToggleEditFormInputs(inputs, paragraphs, false, true);
                    UpdateEditPanelIcons(icons);
                });
            });

            e.stopImmediatePropagation();
        })
    })

    paragraphs.forEach(p => {
        p.addEventListener('dblclick', (e) => {
            e.target.classList.add('d-none');
            e.target.previousElementSibling.hidden = false;
            e.target.previousElementSibling.focus();
            currentParagraphValue = p.innerHTML;
            e.stopImmediatePropagation();
        })
    })

    inputs.forEach(input => {
        input.addEventListener('focusout', InputFocusEventListener)
    })

    erasers.forEach(eraser => {
        eraser.addEventListener('click', (e) => {
            CloseNewNameMappingForm(function () {
                CloseExistingEditForms(function () {
                    document.getElementById('deleteNameMappingButton').onclick = function () {
                        var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
                        $.ajax({
                            data: pageValues,
                            type: "POST",
                            url: `PluginsRename?handler=DeleteNameMapping&Id=${eraser.id}`,
                            success: AjaxSuccessCallback
                        })
                    }

                    $('#deleteNameMappingModal').modal('show');
                });
            });

            e.stopImmediatePropagation();
        })
    })
}

function RedirectTo(goToPage) {
    CloseNewNameMappingForm(function () {
        CloseExistingEditForms(function () {
            window.location.href = goToPage;
        });
    });
}


function CloseExistingEditForms(toggleCallback) {
    var openInputs = document.querySelectorAll('.name-mapping-input-open');
    var hiddenParagraphs = document.querySelectorAll('.name-mapping-closed');
    var editCells = [];

    if (openInputs.length > 0) {
        editCells = openInputs[0].parentElement.parentElement.querySelectorAll('.icon-cell i');
    }

    ToggleEditForm([openInputs, hiddenParagraphs, editCells], toggleCallback, $('#confirmDiscardNameMapping'), document.getElementById('confirmDiscardChangesButton'));
}

function GetCurrentMappingData(inputs) {
    return inputs.length == 0 ? '' : Array(...inputs).reduce(function (prev, next, i) { return prev.value + next.value; });
}

function ToggleEditFormInputs(inputs, paragraphs, restoreChanges, isOpen) {
    oldData = '';
    for (let j = 0; j < inputs.length; j++) {
        if (restoreChanges) {
            inputs[j].value = paragraphs[j].innerHTML;
        }

        inputs[j].classList.remove('name-mapping-input-open');
        paragraphs[j].classList.remove('name-mapping-closed');
        paragraphs[j].classList.toggle('d-none');

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
    }
}

function UpdateEditPanelIcons(editIcons) {
    for (let cell of editIcons) {
        cell.classList.toggle('d-none');
    }
}

function ToggleEditForm(curentRowElements, callback, confirmationModalId, confirmationBtn) {
    const [inputs, paragraphs, icons] = curentRowElements;

    if (oldData == GetCurrentMappingData(inputs)) {
        ToggleEditFormInputs(inputs, paragraphs, false, false);
        UpdateEditPanelIcons(icons);
        callback();
        return;
    }

    confirmationBtn.onclick = function () {
        ToggleEditFormInputs(inputs, paragraphs, true, false);
        UpdateEditPanelIcons(icons);
        callback();
    }

    $(confirmationModalId).modal('show');
    oldData = '';
}

function GetCurrentRowElements(element) {
    return [
        element.parentElement.parentElement.querySelectorAll('.editable-field .name-mapping-input'),
        element.parentElement.parentElement.querySelectorAll('.editable-field .name-mapping'),
        element.parentElement.parentElement.querySelectorAll('.icon-cell i')
    ];
}

function InputFocusEventListener(e) {
    {
        if (currentParagraphValue != e.target.value) {
            UpdateNamesMapping();
        }

        e.target.hidden = true;
        e.target.nextElementSibling.classList.remove('d-none');
        e.stopImmediatePropagation();
    }
}

function UpdateNamesMapping() {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=UpdateNamesMapping`,
        success: AjaxSuccessCallback
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

    if (!newNameMappingForm) {
        confirmationCallback();
        return;
    }

    var inputs = newNameMappingForm.querySelectorAll('.editable-field input');
    if (GetCurrentMappingData(inputs) == '') {
        newNameMappingForm.remove();
        confirmationCallback();
        return;
    }

    document.getElementById('confirmDiscardChangesButton').onclick = function () {
        newNameMappingForm.remove();
        confirmationCallback();
    }

    $('#confirmDiscardNameMapping').modal('show');
}

function AddNameMapping() {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=AddNameMapping`,
        success: AjaxSuccessCallback
    })
}


function DeleteNameMapping(id) {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `PluginsRename?handler=DeleteNameMapping&Id=${id}`,
        success: AjaxSuccessCallback
    })
}

function AjaxSuccessCallback(modalPartialView) {
    if (modalPartialView.includes("DOCTYPE")) {
        location.reload();
    }
    else {
        $('#modalContainer').html(modalPartialView);
        $('#modalContainer').find('.modal').modal('show');
    }
}