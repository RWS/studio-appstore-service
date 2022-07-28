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

function LoadSettingsPage() {

    var navLinks = document.querySelectorAll('#settings-navbar .nav-link');
    var settings = document.querySelectorAll('#settings-details-container .settings-details');
    var erasers = document.querySelectorAll('.table-row .name-mapping-eraser');
    var adders = document.querySelectorAll('.fa-plus-circle');
    var paragraphs = document.querySelectorAll('.name-mapping');
    var inputs = document.querySelectorAll('.name-mapping-input');
    var currentParagraphValue;

    for (let i = 0; i < navLinks.length; i++) {
        navLinks[i].addEventListener('click', function (event) {
            for (let j = 0; j < navLinks.length; j++) {
                settings[j].classList.add('d-none')
                navLinks[j].classList.remove('active')
            }

            navLinks[i].classList.toggle('active');
            settings[i].classList.toggle('d-none');
            event.stopImmediatePropagation();
        })
    };

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
        input.addEventListener('focusout', (e) => {
            if (currentParagraphValue != e.target.value) {
                var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
                var placeholderElement = $('#modalContainer');

                $.ajax({
                    data: pageValues,
                    type: "POST",
                    url: `Settings?handler=UpdateNamesMapping`,
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

            e.target.hidden = true;
            e.target.parentElement.children[1].classList.remove('d-none');
            e.stopImmediatePropagation();
        })
    })

    adders.forEach(adder => {
        adder.addEventListener('click', () => {
            var pageValues = $('#namesMapping').find('select, textarea, input').serialize();

            $.ajax({
                data: pageValues,
                type: "POST",
                url: `Settings?handler=AddNewNameMapping`,
                success: function (partialView) {
                    $("#newNameMappingPartial").html(partialView);
                }
            })
        })
    })

    erasers.forEach(eraser => {
        eraser.addEventListener('click', (e) => {
            var deleteNameMappingButton = document.getElementById('deleteNameMappingButton');
            if (document.getElementById("newNameMappingRow") != null) {
                document.getElementById("newNameMappingRow").remove();
            }

            deleteNameMappingButton.onclick = function ()
            {
                var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
                $.ajax({
                    data: pageValues,
                    type: "POST",
                    url: `Settings?handler=DeleteNameMapping&Id=${eraser.id}`,
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

function CloseNewNameMappingForm() {
    document.getElementById("newNameMappingRow").remove();
}

function AddNewNameMapping() {
    var pageValues = $('#namesMapping').find('select, textarea, input').serialize();
    var placeholderElement = $('#modalContainer');

    $.ajax({
        data: pageValues,
        type: "POST",
        url: `Settings?handler=AddNameMapping`,
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
        url: `Settings?handler=DeleteNameMapping&Id=${id}`,
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
        url: "Settings?handler=ImportFile",
        success: function (modalPartialView) {
            placeholderElement.html(modalPartialView);
            placeholderElement.find('.modal').modal('show');
        }
    });
}


