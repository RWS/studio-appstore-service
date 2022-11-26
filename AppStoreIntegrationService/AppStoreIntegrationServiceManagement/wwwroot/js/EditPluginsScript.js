let parentProducts;

document.addEventListener('DOMContentLoaded', function () {
    var deleteBtns = document.querySelectorAll('#selectedVersion .fa-trash-alt');
    deleteBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            document.getElementById('confirmationBtn').onclick = function () {
                var pageValues = $('main').find('input').serialize();

                $.ajax({
                    data: pageValues,
                    type: "POST",
                    url: `/Version/Delete/${btn.id}`,
                    success: function () {
                        location.reload();
                    }
                })
            }

            document.querySelector("#confirmationModal .modal-body").innerHTML = "Are you sure you want to delete this plugin version?";
            $('#confirmationModal').modal('show');
        })
    })

    document.querySelector('.edit-area').innerHTML = document.getElementById("Description").value;
    let dropDown = new DropDown("#dropDownToggle", "#CategoriesSelect", "#Categories", ".selection-summary", ".overflow-arrow", "#categoriesDropdown", []);
    dropDown.Init();

    $.validator.setDefaults({
        ignore: '.ignore'
    });
});

function SavePlugin() {
    document.getElementById("Description").value = document.querySelector('.edit-area').innerHTML;
    var isNavigationLink = document.getElementById("IsNavigationLink");

    if (isNavigationLink) {
        $("#FileHash").rules(isNavigationLink.checked ? "remove" : "add", "required");
    }

    $("#form").validate();

    if ($("#form").valid()) {
        var pageValues = $('main').find('select, textarea, input').serialize();

        $.ajax({
            data: pageValues,
            type: "POST",
            url: "/Plugins/Plugins/Update",
            success: AjaxSuccessCallback
        })
    }
}

function AddNewVersion() {
    $.ajax({
        type: "POST",
        url: "/Plugins/Version/Add",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
            $('#form').data('validator', null);
            $.validator.unobtrusive.parse('#form');
            document.getElementById("manifestModalBtn").hidden = false;

            let dropDown = new DropDown(
                "#dropDownToggle",
                "#SupportedProducts",
                ".selection-summary",
                ".overflow-arrow",
                "#productsDropdown",
                parentProducts.map(p => p.parentProductName)
            );
            dropDown.Init();
        }
    })
}

function ShowVersionDetails(versionId) {
    document.querySelectorAll(".version-name-label").forEach(label => {
        label.classList.remove("fw-bold");
    })
    event.target.classList.add("fw-bold");

    document.getElementById("selectedVersionId").value = versionId;
    var pageValues = $('main').find('select, textarea, input').serialize();

    $.ajax({
        data: pageValues,
        type: "POST",
        url: "/Plugins/Version/Show",
        success: function (partialView) {
            $('#pluginVersionContainer').html(partialView);
            $('#form').data('validator', null);
            $.validator.unobtrusive.parse('#form');
            document.getElementById("manifestModalBtn").hidden = false;

            let dropDown = new DropDown(
                "#dropDownToggle",
                "#ProductsSelect",
                "#SupportedProducts",
                ".selection-summary",
                ".overflow-arrow",
                "#productsDropdown",
                parentProducts.map(p => p.parentProductName)
            );
            dropDown.Init();
        }
    })
}

function AjaxSuccessCallback(actionResult) {
    if (!actionResult.includes("div")) {
        window.location.href = actionResult;
        return;
    }

    $('.alert').remove();
    $('#statusMessageContainer').html(actionResult);
    $('#statusMessageContainer').find('.modal').modal('show');
    $(".alert").fadeTo(3000, 500).slideUp(500, function () {
        $(this).remove();
    });
}