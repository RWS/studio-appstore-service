function ApplyFiltersOnEnterKey() {
    var expectedKey = 'Enter';
    if (event.key == expectedKey) {
        ApplyFilters('search', 'status');
    }
}

function ApplyFilters(firstParameter, secondParameter) {
    var urlSearchParams = new URLSearchParams(window.location.search);
    var urlParameters = "?";
    if (urlSearchParams.has(secondParameter)) {
        urlParameters += (secondParameter + "=" + urlSearchParams.get(secondParameter) + "&");
    }

    urlParameters += (firstParameter + "=" + encodeURIComponent(document.getElementById(firstParameter).value));
    window.location.href = urlParameters;
}

function DeleteFilter(filterToDelete) {
    var urlSearchParams = new URLSearchParams(window.location.search);
    if (Array.from(urlSearchParams).length == 1) {
        window.location.href = window.location.origin + "/ConfigTool";
        return;
    }

    urlSearchParams.delete(filterToDelete);
    var newUrlParameters = "?";
    urlSearchParams.forEach((value, filter) => {
        newUrlParameters += (filter + "=" + value + "&");
    });

    window.location.href = newUrlParameters.slice(0, -1);
}

function CheckForActiveFilters() {
    var urlSearchParams = new URLSearchParams(window.location.search);
    if (Array.from(urlSearchParams).length > 1) {
        document.getElementById('clearAllFilter').style.display = 'inline-block';
    }

    if (urlSearchParams.has('status')) {
        var statusFilter = urlSearchParams.get('status');
        var statusText = statusFilter == 0 ? "Active" : statusFilter == 1 ? "Inactive" : "All";
        SetFilterValue('clearStatusFilter', 'labelStatusFilter', statusText);
    }

    if (urlSearchParams.has('search')) {
        var searchFilter = urlSearchParams.get('search');
        var searchText = searchFilter.charAt(0).toUpperCase() + searchFilter.slice(1).toLowerCase();
        SetFilterValue('clearSearchFilter', 'labelSearchFilter', searchText);
    }
}

function SetFilterValue(filterTarget, labelTarget, targetValue) {
    document.getElementById(filterTarget).style.display = 'inline-block';
    document.getElementById(labelTarget).innerHTML = targetValue;
}

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
            if (modalPartialView.includes("DOCTYPE"))
            {
                window.location.href = goToPage;
            }
            else
            {
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
//settings page fields toggle
var navLinks = document.querySelectorAll('#settings-navbar .nav-link');
var settings = document.querySelectorAll('#settings-details-container .settings-details');

for (let i = 0; i < navLinks.length; i++) {
    navLinks[i].addEventListener('click', function (event) {
        for (let j = 0; j < navLinks.length; j++) {
            settings[j].classList.add('d-none')
            navLinks[j].classList.remove('active')
        }

        this.classList.toggle('active');
        settings[i].classList.toggle('d-none');
        event.stopImmediatePropagation();
    })
};


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


