let x;
let y;
let draggable;

function EnsurePreserved(callback) {
    let data = new FormData(document.getElementById("form"));
    let request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            if (request.responseText.includes('div')) {
                $('#modalContainer').html(request.responseText);
                $('#modalContainer').find('.modal').modal('show');
                document.getElementById("modalContainer").querySelector("#confirmationBtn").addEventListener('click', () => {
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
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    let alert = document.querySelector('.alert')

    if (alert) {
        setTimeout(() => {

            alert.classList.add('slide-right');
            alert.addEventListener('animationend', () => {
                document.querySelector('.alert-container').remove();
            })
        }, 3000);
    }
});

function BeginDrag() {
    x = event.clientX;
    y = event.clientY;
    draggable = event.currentTarget.parentElement;
    document.addEventListener('mousemove', Drag);
    document.addEventListener('mouseup', StopDrag);
}

function Drag(e) {
    const dx = e.clientX - x;
    const dy = e.clientY - y;

    draggable.style.top = `${draggable.offsetTop + dy}px`;
    draggable.style.left = `${draggable.offsetLeft + dx}px`;

    x = e.clientX;
    y = e.clientY;
};

function StopDrag() {
    document.removeEventListener('mousemove', Drag);
    document.removeEventListener('mouseup', StopDrag);
};

function UpdateDescription() {
    var editor = event.target;
    editor.parentElement.nextElementSibling.value = editor.innerHTML;
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