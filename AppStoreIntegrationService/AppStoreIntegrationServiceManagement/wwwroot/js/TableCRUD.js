class TableCrud {

    #field;
    #updateRoute;
    #additionRoute;
    #deletionRoute;

    constructor(field, updateRoute, additionRoute, deletionRoute) {
        this.#field = field;
        this.#updateRoute = updateRoute;
        this.#additionRoute = additionRoute;
        this.#deletionRoute = deletionRoute;
    }

    Edit(id) {
        let location = window.location.href;
        let url = new URL(location);

        if (location.includes('?')) {
            url = new URL(location.split('?')[0]);
        }

        url.searchParams.set(`selected${this.#field}`, id);
        window.location.href = url.href;
    }

    Discard() {
        let url = window.location.href;

        if (url.includes('?')) {
            window.location.href = url.split('?')[0];
            return;
        }

        window.location.reload();
    }

    Save() {
        $('#form').validate();

        if ($('#form').valid()) {
            let request = new XMLHttpRequest();
            let data = new FormData(document.getElementById("form"));

            request.onreadystatechange = () => {
                if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                    if (request.responseText.includes('div')) {
                        this.AjaxSuccessCallback(request.responseText);
                        return;
                    }

                    this.Discard()
                }
            }

            request.open("POST", `/Settings/${this.#updateRoute}`);
            request.send(data);
        }
    }

    Add() {
        let request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                document.getElementById("newRowPartial").innerHTML = request.responseText;
            }
        }

        request.open("POST", `/Settings/${this.#additionRoute}`);
        request.send();
    }

    Delete(id) {
        document.getElementById("confirmationBtn").addEventListener('click', () => {
            let request = new XMLHttpRequest();

            request.onreadystatechange = function () {
                if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                    window.location.reload();
                }
            }

            request.open("POST", `/Settings/${this.#deletionRoute}/${id}`);
            request.send();
        })
    }

    AjaxSuccessCallback(actionResult) {
        $('.alert').remove();
        $('#statusMessageContainer').html(actionResult);
        $('#statusMessageContainer').find('.modal').modal('show');
        $(".alert").fadeTo(3000, 500).slideUp(500, function () {
            $(this).remove();
        });
    }
}
