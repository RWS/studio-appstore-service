﻿let parentProducts;

function AddPlugin() {
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
            url: "/Plugins/Plugins/Create",
            success: function (actionResult) {
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
                document.querySelector("#productsDropdown #dropDownToggle"),
                document.querySelector("#productsDropdown #ProductsSelect"),
                $("#productsDropdown #SupportedProducts"),
                document.querySelector("#productsDropdown .selection-summary"),
                document.querySelectorAll("#productsDropdown .overflow-arrow"),
                parentProducts.map(p => p.parentProductName)
            );
            dropDown.Init();
        }
    })
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelector('.edit-area').innerHTML = document.getElementById("Description").value;

    let dropDown = new DropDown(
        document.querySelector("#categoriesDropdown #dropDownToggle"),
        document.querySelector("#categoriesDropdown #CategoriesSelect"),
        $("#categoriesDropdown #Categories"),
        document.querySelector("#categoriesDropdown .selection-summary"),
        document.querySelectorAll("#categoriesDropdown .overflow-arrow"),
        []
    );
    dropDown.Init();

    $.validator.setDefaults({
        ignore: []
    });
})