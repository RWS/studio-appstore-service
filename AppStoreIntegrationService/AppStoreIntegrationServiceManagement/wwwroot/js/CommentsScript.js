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

            editor = new Quill('#editor', {
                modules:
                {
                    toolbar: quillToolbarOptions,
                },
                theme: 'snow'
            });
        }
    }

    request.open("POST", `/Plugins/Comments/New`);
    request.send(data);
}

function SaveComment() {
    let request = new XMLHttpRequest();
    let description = document.querySelector("#editor .ql-editor").innerHTML;
    document.getElementById("CommentDescription").innerText = description;

    $("#form").validate();

    if ($("#form").valid) {
        let data = new FormData(document.getElementById("form"));
        ToggleLoader(event.currentTarget);

        request.onreadystatechange = function () {
            if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
                DiscardCommentEdit();
            }
        }

        request.open("POST", `/Plugins/Comments/Update`);
        request.send(data);
    }
}

function DeleteComment(pluginId, commentId, versionId = '') {
    document.getElementById("confirmationBtn").addEventListener('click', () => {
        let data = new FormData();

        data.set("PluginId", pluginId);
        data.set("VersionId", versionId);
        data.set("CommentId", commentId);

        SendPostRequest('/Plugins/Comments/Delete', data)
    })
}