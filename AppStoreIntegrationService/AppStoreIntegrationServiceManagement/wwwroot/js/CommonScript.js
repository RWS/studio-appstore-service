let x;
let y;
let draggable;

function RedirectTo(goToPage, controller, action) {
    var currentPage = controller + '/' + action;

    if (currentPage == "Plugins/Edit" || currentPage == "Plugins/New") {
        document.getElementById("Description").value = document.querySelector('.edit-area').innerHTML;
        CreateRequest($('main').find('select, textarea, input').serialize(), `/Plugins/GoToPage/${goToPage}/${action}`);
        return;
    }

    if (controller == "PluginsRename") {
        CreateRequest($('main').find('input').serialize(), `/PluginsRename/GoToPage/${goToPage}`);
        return;
    }

    if (controller == "Products") {
        CreateRequest($('main').find('input, select').serialize(), `/Products/GoToPage/${goToPage}`);
        return;
    }

    if (controller == "Categories") {
        CreateRequest($('main').find('input, select').serialize(), `/Categories/GoToPage/${goToPage}`);
        return;
    }

    if (controller == "Account" && action != "Users") {
        CreateRequest($('main').find("input, input[type='radio']").serialize(), `/Account/GoToPage/${goToPage}/${action}`);
        return;
    }

    goToPage = goToPage.replaceAll('.', '\\');
    window.location.href = `${goToPage}`;
}

function CreateRequest(pageValues, url) {
    $.ajax({
        data: pageValues,
        type: "POST",
        url: url,
        success: function (actionResult) {
            if (!actionResult.includes("div")) {
                window.location.href = actionResult;
            }
            else {
                $('#modalContainer').html(actionResult);
                $('#modalContainer').find('.modal').modal('show');
            }
        }
    })
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

    $(".alert-danger, .alert-success").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
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

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.href = actionResult;
        return;
    }

    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}