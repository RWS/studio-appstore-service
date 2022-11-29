let fileHash;
let manifestCompare;

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

function GenerateChecksum() {
    var pageValues = $('main').find('select, textarea, input').serialize();
    var button = event.target;
    $("#VersionDownloadUrl").validate();

    if ($("#VersionDownloadUrl").valid()) {
        button.disabled = true;
        button.firstElementChild.hidden = false;

        $.ajax({
            data: pageValues,
            type: "POST",
            url: "/Plugins/Version/GenerateChecksum",
            success: function (result) {
                button.disabled = false;
                button.firstElementChild.hidden = true;
                AjaxSuccessCallback(result);
                document.getElementById("FileHash").value = fileHash;
            }
        })
    }
}

function ManageChecksum(url, checksum) {
    var currentInput = event.target.value;

    if (url != currentInput) {
        document.getElementById("FileHash").value = "";
        return;

    }

    document.getElementById("FileHash").value = checksum;
}

document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
});

function CompareWithManifest() {
    $("#VersionDownloadUrl").validate()
    if ($("#IsNavigationLink")[0].checked || !$("#VersionDownloadUrl").valid()) {
        return;
    }

    $("#SupportedProducts").validate();

    if (!$("#SupportedProducts").valid()) {
        return;
    }

    var button = event.target;
    button.disabled = true;
    button.firstElementChild.hidden = false;
    let data = $('main').find('select, textarea, input').serialize();

    $.ajax({
        data: data,
        type: "POST",
        url: "/Plugins/Plugins/ManifestCompare",
        success: function (actionResult) {
            AjaxSuccessCallback(actionResult)
            if (manifestCompare) {
                document.getElementById("PluginNameManifestConflict").hidden = manifestCompare.isNameMatch;
                document.getElementById("DeveloperNameManifestConflict").hidden = manifestCompare.isAuthorMatch;
                document.getElementById("VersionNumberManifestConflict").hidden = manifestCompare.isVersionMatch;
                document.getElementById("MinVersionManifestConflict").hidden = manifestCompare.isMinVersionMatch;
                document.getElementById("MaxVersionManifestConflict").hidden = manifestCompare.isMaxVersionMatch;
                document.getElementById("ProductManifestConflict").hidden = manifestCompare.isProductMatch;
                document.getElementById("SuccessManifestCompare").hidden = !manifestCompare.isFullMatch;
                document.getElementById("FailManifestCompare").hidden = manifestCompare.isFullMatch;

                document.querySelectorAll(".manifest-field").forEach(field => {
                    field.addEventListener('input', () => {
                        field.parentElement.lastElementChild.hidden = true;
                    })
                })
            }
            
            button.disabled = false;
            button.firstElementChild.hidden = true; 
        }
    });
}

function ChangeFontSize() {
    document.documentElement.style.setProperty('--pa-admin-fontsize', `${event.target.value}px`);
    document.cookie = `FontSize=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function RestorePreferences() {
    document.cookie = "FontSize=; Path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"
    document.cookie = "FontFamily=; Path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"
    document.cookie = "HoverColor=; Path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"
    document.cookie = "BackgroundColor=; Path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"
    document.cookie = "ForegroundColor=; Path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"
    document.cookie = "LogoImage=; Path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT"
    window.location.reload();
}

function ChangeFontFamily() {
    let container = document.querySelector("head");
    let link = container.querySelector("#GoogleFont");
    if (link) {
        container.removeChild(link);
    }

    link = document.createElement("link");
    link.href = `https://fonts.googleapis.com/css2?family=${event.target.value}&display=swap`
    link.rel = "stylesheet"
    link.id = "GoogleFont"
    container.append(link);
    document.documentElement.style.setProperty('--pa-admin-fontfamily', `${event.target.value}`);
    document.cookie = `FontFamily=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`
}

function ChangeBackground() {
    document.documentElement.style.setProperty('--pa-admin-color', `${event.target.value}`);
    document.documentElement.style.setProperty('--pa-admin-color-hover', `${event.target.value}`);
    document.cookie = `BackgroundColor=${event.target.value}; HoverColor=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function ChangeForeground() {
    document.documentElement.style.setProperty('--pa-admin-foreground', `${event.target.value}`);
    document.cookie = `ForegroundColor=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

function ChangeLogo() {
    console.log(event.target.value)
    document.querySelector("#LogoImage").src = event.target.value;
    document.querySelector("#LogoImage").width = 75;
    document.cookie = `LogoImage=${event.target.value}; expires=${new Date().setMonth(new Date().getMonth + 6)}; Path=/;`;
}

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