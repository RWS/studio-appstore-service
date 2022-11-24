let currentParagraphValue;
let oldData = '';
let newData = '';
let oldCheckboxValue = {}

class TableCrud {
    #deleteUrl;
    #saveUrl;
    #updateUrl;
    #addNewUrl;
    #container;

    constructor(deleteUrl, saveUrl, updateUrl, addNewUrl, container) {
        this.#deleteUrl = deleteUrl;
        this.#saveUrl = saveUrl;
        this.#updateUrl = updateUrl;
        this.#addNewUrl = addNewUrl;
        this.#container = container;
    }

    DiscardChanges() {
        this.#ToggleEditForm(this.#GetCurrentRowElements(event.target, false), () => { return; }, '#confirmationModal', document.getElementById('confirmationBtn'), null);
    }

    ApplyChanges() {
        this.#ToggleEditForm(this.#GetCurrentRowElements(event.target, false), () => { return; }, '#confirmationModal', document.getElementById('dismissBtn'), document.getElementById('confirmationBtn'));
    }

    FocusOut() {
        let inputValue = event.target.type == "checkbox" ? event.target.checked : event.target.value;
        if (currentParagraphValue != inputValue) {
            this.#UpdateTable();
        }

        event.target.hidden = true;
        event.target.nextElementSibling.classList.remove('d-none');
    }

    EditRow() {
        const [inputs, paragraphs, icons] = this.#GetCurrentRowElements(event.target, false);

        this.CloseNewRowForm(() => {
            this.#CloseExistingEditForms(() => {
                this.#ToggleEditFormInputs(inputs, paragraphs, false, true);
                this.#UpdateEditPanelIcons(icons);
            });
        });
    }

    DeleteRow() {
        var productId = event.target.id;
        this.CloseNewRowForm(() => {
            this.#CloseExistingEditForms(() => {
                document.getElementById('confirmationBtn').onclick = () => {
                    $.ajax({
                        type: "POST",
                        url: this.#deleteUrl + productId,
                        success: this.#AjaxSuccessCallback
                    })
                }

                document.querySelector("#confirmationModal .modal-body").innerHTML = "Are you sure you want to delete this item?";
                $('#confirmationModal').modal('show');
            });
        });
    }

    ChangeParentProduct() {
        var pageValues = $('main').find('input, select').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: this.#updateUrl,
            success: this.#AjaxSuccessCallback
        })
    }

    EditCell() {
        const [inputs, paragraphs, icons] = this.#GetCurrentRowElements(event.currentTarget, true);
        this.CloseNewRowForm(() => {
            this.#CloseExistingEditForms(() => {
                this.#ToggleEditFormInputs(inputs, paragraphs, false, false);
            });
        });

        event.currentTarget.classList.add('d-none');
        event.currentTarget.previousElementSibling.hidden = false;
        event.currentTarget.previousElementSibling.focus();
        currentParagraphValue = event.currentTarget.previousElementSibling.type == "checkbox" ? event.currentTarget.previousElementSibling.checked : event.currentTarget.previousElementSibling.value;
    }

    CloseNewRowForm(confirmationCallback) {
        var newNameMappingForm = document.querySelector(this.#container + " #newDataRow");

        if (!newNameMappingForm) {
            confirmationCallback();
            return;
        }

        var inputs = newNameMappingForm.querySelectorAll('.editable-field input:first-child');
        if (this.#GetCurrentRowData(inputs) == '' || this.#GetCurrentRowData(inputs) == 'false') {
            newNameMappingForm.remove();
            confirmationCallback();
            return;
        }

        document.getElementById('confirmationBtn').onclick = () => {
            newNameMappingForm.remove();
            confirmationCallback();
        }

        document.querySelector("#confirmationModal .modal-body").innerHTML = "Discard changes for new item?"
        $('#confirmationModal').modal('show');
    }

    SaveNewRowData() {
        var pageValues = $('main').find('input, select').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: this.#saveUrl,
            success: this.#AjaxSuccessCallback
        })
    }

    AddNewRow() {
        var pageValues = $('main').find('input').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: this.#addNewUrl,
            success: (partialView) => {
                this.#CloseExistingEditForms(() => {
                    $(this.#container + " #newRowPartial").html(partialView);
                    window.scrollTo(0, document.querySelector(this.#container + " #newRowPartial").getBoundingClientRect().y);
                })
            }
        })
    }

    Change() {
        this.#ToggleCheckGroup(event.target, event.target.checked);
    }

    #UpdateTable() {
        var pageValues = $('main').find('input, select').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: this.#updateUrl,
            success: this.#AjaxSuccessCallback
        })
    }

    #ToggleEditForm(curentRowElements, callback, confirmationModalId, confirmationBtn, dismissBtn) {
        const [inputs, paragraphs, icons] = curentRowElements;

        if (oldData == this.#GetCurrentRowData(inputs)) {
            this.#ToggleEditFormInputs(inputs, paragraphs, false, false);
            this.#UpdateEditPanelIcons(icons);
            callback();
            return;
        }

        var modalBody = document.querySelector("#confirmationModal .modal-body");
        modalBody.innerHTML = "Discard changes for this item?";

        if (dismissBtn != null) {
            modalBody.innerHTML = "Keep changes for this item?";
            dismissBtn.onclick = () => { this.#UpdateTable() };
        }

        confirmationBtn.onclick = () => {
            oldData = '';
            this.#ToggleEditFormInputs(inputs, paragraphs, true, false);
            this.#UpdateEditPanelIcons(icons);
            callback();
        }

        $(confirmationModalId).modal('show');
    }

    #CloseExistingEditForms(toggleCallback) {
        var openInputs = document.querySelectorAll('.entry-open');
        var hiddenParagraphs = document.querySelectorAll('.section-closed');
        var editCells = [];

        if (openInputs.length > 0) {
            editCells = openInputs[0].closest('.table-row').querySelectorAll('.icon-cell i');
        }

        this.#ToggleEditForm([openInputs, hiddenParagraphs, editCells], toggleCallback, $('#confirmationModal'), document.getElementById('confirmationBtn'), null);
    }

    #GetCurrentRowData(inputs) {
        var result = '';

        for (let input of inputs) {
            result += input.type == "checkbox" ? input.checked : input.value;
        }

        return result;
    }

    #ToggleEditFormInputs(inputs, paragraphs, restoreChanges, isOpen) {
        oldData = '';
        for (let j = 0; j < inputs.length; j++) {
            if (restoreChanges) {
                if (inputs[j].type == "checkbox") {
                    this.#ToggleCheckGroup(inputs[j], oldCheckboxValue[`input-${j}`]);
                    delete oldCheckboxValue[`input-${j}`]
                }
                inputs[j].value = paragraphs[j].innerHTML;
            }

            inputs[j].classList.remove('entry-open');
            paragraphs[j].classList.remove('section-closed');
            paragraphs[j].classList.toggle('d-none');

            if (isOpen) {
                inputs[j].hidden = false;
                inputs[j].classList.add('entry-open');
                this.#ToggleAttribute(inputs[j], isOpen);
                paragraphs[j].classList.add('section-closed');
                oldData += inputs[j].type == "checkbox" ? inputs[j].checked : inputs[j].value;
                if (inputs[j].type == "checkbox") {
                    oldCheckboxValue[`input-${j}`] = inputs[j].checked;
                }
            } else {
                inputs[j].hidden = true;
                this.#ToggleAttribute(inputs[j], isOpen)
            }
        }

        if (isOpen) {
            document.querySelector('.editable-field .autofocus.entry-open').focus();
        }
    }

    #ToggleAttribute(input, isOpen) {
        if (isOpen) {
            input.setAttribute('onfocusout', null);
            if (input.type == "select-one") {
                input.setAttribute('onchange', null);
            }
            return;
        }

        input.setAttribute('onfocusout', 'table.FocusOut()');
        if (input.type == "select-one") {
            input.setAttribute('onchange', 'table.ChangeParentProduct()');
        }
    }

    #UpdateEditPanelIcons(editIcons) {
        for (let cell of editIcons) {
            cell.classList.toggle('d-none');
        }
    }

    #GetCurrentRowElements(element, isDoubleClickMode) {
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

    #AjaxSuccessCallback(actionResult) {
        if (!actionResult.includes("div")) {
            window.location.href = actionResult;
        }

        $('#statusMessageContainer').html(actionResult);
        $('#statusMessageContainer').find('.modal').modal('show');
        $(".alert").fadeTo(3000, 500).slideUp(500, function () {
            $(this).remove();
        });
    }

    #ToggleCheckGroup(input, isChecked) {
        var icon = input.nextElementSibling.firstChild;
        if (icon != null) {
            input.checked = isChecked;
            icon.classList.remove(`fa-${isChecked ? 'times' : 'check'}-circle`, `text-${isChecked ? 'danger' : 'success'}`);
            icon.classList.add(`fa-${isChecked ? 'check' : 'times'}-circle`, `text-${isChecked ? 'success' : 'danger'}`);
        }
    }
}