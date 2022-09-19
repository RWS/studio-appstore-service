let currentParagraphValue;
let oldData = '';
let newData = '';
let oldCheckboxValue = '';

class UrlStorage {
    constructor(deleteUrl, saveUrl, updateUrl, addNewUrl) {
        this.deleteUrl = deleteUrl;
        this.saveUrl = saveUrl;
        this.updateUrl = updateUrl;
        this.addNewUrl = addNewUrl;
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const erasers = document.querySelectorAll('.table-row .row-eraser');
    const paragraphs = document.querySelectorAll('.editable-field .section');
    const inputs = document.querySelectorAll('.editable-field .entry');
    const editers = document.querySelectorAll('.icon-cell .fa-pen-alt');
    const checkMarks = document.querySelectorAll('.icon-cell .fa-check-circle');
    const discardBtns = document.querySelectorAll('.icon-cell .fa-times-circle');

    discardBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            ToggleEditForm(GetCurrentRowElements(btn, false), function () { return; }, '#confirmationModal', document.getElementById('confirmationBtn'), null);
            e.stopImmediatePropagation();
        })
    })

    checkMarks.forEach(mark => {
        mark.addEventListener('click', (e) => {
            ToggleEditForm(GetCurrentRowElements(mark, false), function () { return; }, '#confirmationModal', document.getElementById('dismissBtn'), document.getElementById('confirmationBtn'));
            e.stopImmediatePropagation();
        })
    });

    editers.forEach(editer => {
        editer.addEventListener('click', (e) => {
            const [inputs, paragraphs, icons] = GetCurrentRowElements(editer, false);

            CloseNewRowForm(function () {
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
            const [inputs, paragraphs, icons] = GetCurrentRowElements(p, true);
            CloseNewRowForm(function () {
                CloseExistingEditForms(function () {
                    ToggleEditFormInputs(inputs, paragraphs, false, false);
                });
            });

            p.classList.add('d-none');
            p.previousElementSibling.hidden = false;
            p.previousElementSibling.focus();
            currentParagraphValue = p.previousElementSibling.type == "checkbox" ? p.previousElementSibling.checked : p.previousElementSibling.value;
            e.stopImmediatePropagation();
        })
    })

    inputs.forEach(input => {
        input.addEventListener('focusout', InputFocusEventListener)
    })

    erasers.forEach(eraser => {
        eraser.addEventListener('click', (e) => {
            CloseNewRowForm(function () {
                CloseExistingEditForms(function () {
                    document.getElementById('confirmationBtn').onclick = function () {
                        $.ajax({
                            type: "POST",
                            url: urlStorage.deleteUrl + eraser.id,
                            success: AjaxSuccessCallback
                        })
                    }

                    document.querySelector("#confirmationModal .modal-body").innerHTML = "Are you sure you want to delete this item?";
                    $('#confirmationModal').modal('show');
                });
            });

            e.stopImmediatePropagation();
        })
    })
});

function CloseExistingEditForms(toggleCallback) {
    var openInputs = document.querySelectorAll('.entry-open');
    var hiddenParagraphs = document.querySelectorAll('.section-closed');
    var editCells = [];

    if (openInputs.length > 0) {
        editCells = openInputs[0].closest('.table-row').querySelectorAll('.icon-cell i');
    }

    ToggleEditForm([openInputs, hiddenParagraphs, editCells], toggleCallback, $('#confirmationModal'), document.getElementById('confirmationBtn'), null);
}

function GetCurrentRowData(inputs) {
    var result = '';

    for (let input of inputs) {
        result += input.type == "checkbox" ? input.checked : input.value;
    }

    return result;
}

function ToggleEditFormInputs(inputs, paragraphs, restoreChanges, isOpen) {
    oldData = '';
    for (let j = 0; j < inputs.length; j++) {
        if (restoreChanges) {
            if (inputs[j].type == "checkbox") {
                inputs[j].checked = oldCheckboxValue;
            }
            inputs[j].value = paragraphs[j].innerHTML;
        }

        inputs[j].classList.remove('entry-open');
        paragraphs[j].classList.remove('section-closed');
        paragraphs[j].classList.toggle('d-none');

        if (isOpen) {
            inputs[j].hidden = false;
            inputs[j].classList.add('entry-open');
            inputs[j].removeEventListener('focusout', InputFocusEventListener);
            paragraphs[j].classList.add('section-closed');
            oldData += inputs[j].type == "checkbox" ? inputs[j].checked : inputs[j].value;
            if (inputs[j].type == "checkbox") {
                oldCheckboxValue = inputs[j].checked;
            }
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

function ToggleEditForm(curentRowElements, callback, confirmationModalId, confirmationBtn, dismissBtn) {
    const [inputs, paragraphs, icons] = curentRowElements;

    if (oldData == GetCurrentRowData(inputs)) {
        ToggleEditFormInputs(inputs, paragraphs, false, false);
        UpdateEditPanelIcons(icons);
        callback();
        return;
    }

    var modalBody = document.querySelector("#confirmationModal .modal-body");
    modalBody.innerHTML = "Discard changes for this item?";

    if (dismissBtn != null) {
        modalBody.innerHTML = "Keep changes for this item?";
        dismissBtn.onclick = UpdateTable;
    }

    confirmationBtn.onclick = function () {
        ToggleEditFormInputs(inputs, paragraphs, true, false);
        UpdateEditPanelIcons(icons);
        callback();
    }

    $(confirmationModalId).modal('show');
    oldData = '';
}

function GetCurrentRowElements(element, isDoubleClickMode) {
    return isDoubleClickMode ?
        [
            element.parentElement.querySelectorAll('.entry'),
            element.parentElement.querySelectorAll('.section'),
            []
        ] : [
            element.parentElement.parentElement.querySelectorAll('.editable-field .entry'),
            element.parentElement.parentElement.querySelectorAll('.editable-field .section'),
            element.parentElement.parentElement.querySelectorAll('.icon-cell i')
        ];
}

function InputFocusEventListener(e) {
    let inputValue = e.target.type == "checkbox" ? e.target.checked : e.target.value;
    if (currentParagraphValue != inputValue) {
        UpdateTable();
    }

    e.target.hidden = true;
    e.target.nextElementSibling.classList.remove('d-none');
    e.stopImmediatePropagation();
}

function CloseNewRowForm(confirmationCallback) {
    var newNameMappingForm = document.getElementById("newDataRow");

    if (!newNameMappingForm) {
        confirmationCallback();
        return;
    }

    var inputs = newNameMappingForm.querySelectorAll('.editable-field input');
    if (GetCurrentRowData(inputs) == '') {
        newNameMappingForm.remove();
        confirmationCallback();
        return;
    }

    document.getElementById('confirmationBtn').onclick = function () {
        newNameMappingForm.remove();
        confirmationCallback();
    }

    document.querySelector("#confirmationModal .modal-body").innerHTML = "Discard changes for new item?"
    $('#confirmationModal').modal('show');
}

function SaveNewRowData() {
    var pageValues = $('main').find('input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: urlStorage.saveUrl,
        success: AjaxSuccessCallback
    })
}

function AddNewRow() {
    var pageValues = $('main').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: urlStorage.addNewUrl,
        success: function (partialView) {
            $("#newRowPartial").html(partialView);
            CloseExistingEditForms(function () { return; })
            window.scrollTo(0, document.getElementById("newRowPartial").getBoundingClientRect().y);
        }
    })
}

function UpdateTable() {
    var pageValues = $('main').find('input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: urlStorage.updateUrl,
        success: AjaxSuccessCallback
    })
}

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.href = actionResult;
    }

    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}