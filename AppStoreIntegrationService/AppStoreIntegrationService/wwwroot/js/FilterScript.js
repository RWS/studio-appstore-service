function AddFilterOnEnterKey() {
    var expectedKey = 'Enter';
    if (event.key == expectedKey) {
        AddFilter('search');
    }
}

function AddFilter(filterToAdd) {
    var urlSearchParams = new URLSearchParams(window.location.search);
    if (urlSearchParams.has(filterToAdd)) {
        urlSearchParams.delete(filterToAdd);
    }

    var newFilterValue = unescape(encodeURIComponent(document.getElementById(filterToAdd).value)).trim();
    if (newFilterValue != "") {
        urlSearchParams.append(filterToAdd, (newFilterValue.charAt(0).toUpperCase() + newFilterValue.slice(1).toLowerCase()));
    }

    ApplyFilters(urlSearchParams);
}

function DeleteFilter(filterToDelete) {
    var urlSearchParams = new URLSearchParams(window.location.search);
    if (Array.from(urlSearchParams).length == 1) {
        window.location.href = window.location.origin + "/ConfigTool";
        return;
    }

    urlSearchParams.delete(filterToDelete);
    ApplyFilters(urlSearchParams);
}

function ApplyFilters(urlSearchParams) {
    var urlParameters = "?";
    urlSearchParams.forEach((value, filter) => {
        urlParameters += (filter + "=" + value + "&");
    });

    window.location.href = urlParameters.slice(0, -1);
}