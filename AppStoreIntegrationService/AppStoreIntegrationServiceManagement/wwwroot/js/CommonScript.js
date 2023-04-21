let editor = null;
let isReadOnly = false;
let tooltipTriggerList = [];
let quillToolbarOptions = [
    ['bold', 'italic', 'underline', 'strike'],
    ['blockquote', 'code-block'],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    [{ 'script': 'sub' }, { 'script': 'super' }],
    [{ 'indent': '-1' }, { 'indent': '+1' }],
    [{ 'align': [] }],
    ['clean'],
    [{ 'direction': 'rtl' }],
    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
    [{ 'color': [] }, { 'background': [] }],
    ['link', 'image', 'video'],
]

function EnsurePreserved(callback) {
    var textArea = document.querySelector(".text-area-hidden");
    if (textArea) {
        textArea.innerText = document.querySelector("#editor .ql-editor").innerHTML;
    }

    let data = new FormData(document.getElementById("form"));
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            if (request.responseText.includes('div')) {
                HttpRequestCallback(request.responseText);
                document.querySelector("#partialContainer #confirmationBtn").addEventListener('click', () => {
                    callback();
                })

            } else {
                callback();
            }
        }
    }

    request.open("POST", `/Preservation/Check`);
    request.send(data);
}

function LoadNotifications(clearAll, clearStatus, clearQuery) {
    let request = new XMLHttpRequest();
    let data = clearAll ? new FormData() : new FormData(document.getElementById("NotificationForm"));

    if (clearStatus) {
        data.set("NotificationStatus", 0);
        document.getElementById("NotificationStatus").value = null;
    }

    if (clearQuery) {
        data.set("NotificationQuery", "");
        document.getElementById("NotificationQuery").value = null;
    }

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            document.getElementById("notificationsContainer").innerHTML = request.responseText;
        }
    }

    request.open("POST", `/Identity/Notifications/LoadNotifications`);
    request.send(data);
}

function ChangeNotificationStatus(status, id) {
    let request = new XMLHttpRequest();
    let data = new FormData();

    data.set("Id", id);
    data.set("Status", status)

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            window.location.reload();
        }
    }

    request.open("POST", `/Identity/Notifications/ChangeStatus`);
    request.send(data);
}

function Collapse(element) {

    if (window.innerWidth > 992) {
        return;
    }

    element.parentElement.classList.toggle('bg-collapsed');

    var children = element.closest('tr').children;
    for (let child of children) {
        if (child != element.parentElement) {
            child.classList.toggle('d-none');
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    InitTooltips();
    TriggerAlertAnimation();
});

function InitTooltips() {
    let tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');

    tooltips.forEach(tooltip => {
        if (!tooltip.hasAttribute("aria-describedby")) {
            new bootstrap.Tooltip(tooltip)
        }
    })
}

function ToggleLoader(element) {
    if (element.disabled) {
        element.disabled = false;
        element.firstElementChild.hidden = true;
        return;
    }

    element.disabled = true;
    element.firstElementChild.hidden = false;
}

function HttpRequestCallback(response, element) {
    if (!response.includes('div')) {
        window.location.href = response;
        return;
    }

    let modal = document.getElementById("confirmationModal");

    if (modal && modal.classList.contains("show")) {
        bootstrap.Modal.getInstance(modal).hide();
    }

    document.getElementById("partialContainer").innerHTML = response;
    modal = document.querySelector('#partialContainer .modal');

    if (modal) {
        new bootstrap.Modal(modal, { keyboard: false }).show()
    } else {
        TriggerAlertAnimation();
    }

    if (element) {
        ToggleLoader(element);
    }
}

function SendPostRequest(route, data = null) {
    let request = new XMLHttpRequest();
    let element = event.currentTarget;
    ToggleLoader(element);

    request.onreadystatechange = () => {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            HttpRequestCallback(request.responseText, element);
        }
    }

    request.open("POST", route);
    request.send(data);
}

function TriggerAlertAnimation() {
    let alert = document.querySelector('.alert');

    if (alert) {
        setTimeout(() => {

            alert.classList.add('slide-right');
            alert.addEventListener('animationend', () => {
                document.querySelector('.alert-container').remove();
            })
        }, 3000);
    }
}