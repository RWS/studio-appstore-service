function EditComment(commentId) {
    let url = new URL(window.location.href);

    url.searchParams.set("selectedComment", commentId);
    window.location.href = url.href;
}

function DiscardCommentEdit() {
    let url = new URL(window.location.href);

    if (url.searchParams.has("selectedComment")) {
        url.searchParams.delete("selectedComment");
        window.location.href = url.href;
        return;
    }

    window.location.reload();
}

function AddComment(pluginId, versionId = '') {
    let content = event.currentTarget.parentElement;
    let request = new XMLHttpRequest();
    let data = new FormData();

    data.set("PluginId", pluginId);
    data.set("VersionId", versionId);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            content.innerHTML = request.responseText;
            Init();
        }
    }

    request.open("POST", `/Comments/New`);
    request.send(data);
}

function SaveComment() {
    let button = event.currentTarget;
    let request = new XMLHttpRequest();
    let data = new FormData(document.getElementById("form"));
    ToggleLoader(button);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            DiscardCommentEdit();
        }
    }

    request.open("POST", `/Comments/Update`);
    request.send(data);
}

function EditComment(commentId) {
    let url = new URL(window.location.href);

    url.searchParams.set("selectedComment", commentId);
    window.location.href = url.href;
}

function DeleteComment(pluginId, commentId, versionId = '') {
    document.getElementById("confirmationBtn").addEventListener('click', () => {
        let request = new XMLHttpRequest();
        let data = new FormData();

        data.set("PluginId", pluginId);
        data.set("VersionId", versionId);
        data.set("CommentId", commentId);

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                window.location.reload();
            }
        }

        request.open("POST", `/Comments/Delete`);
        request.send(data);
    })
}