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
            var toolbarOptions = [
                ['bold', 'italic', 'underline', 'strike'],
                ['blockquote', 'code-block'],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                [{ 'script': 'sub' }, { 'script': 'super' }],
                [{ 'indent': '-1' }, { 'indent': '+1' }],
                [{ 'align': [] }],
                ['clean'],
                [{ 'direction': 'rtl' }],
                [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
                [{ 'color': [] }, { 'background': [] }],
                ['link', 'image', 'video'],
            ];

            editor = new Quill('#editor', {
                modules:
                {
                    toolbar: toolbarOptions,
                },
                theme: 'snow'
            });
        }
    }

    request.open("POST", `/Plugins/Comments/New`);
    request.send(data);
}

function SaveComment() {
    let button = event.currentTarget;
    let request = new XMLHttpRequest();
    document.getElementById("CommentDescription").innerText = document.querySelector("#editor .ql-editor").innerHTML;
    let data = new FormData(document.getElementById("form"));
    ToggleLoader(button);

    request.onreadystatechange = function () {
        if (request.readyState == XMLHttpRequest.DONE && request.status == 200) {
            DiscardCommentEdit();
        }
    }

    request.open("POST", `/Plugins/Comments/Update`);
    request.send(data);
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

        request.open("POST", `/Plugins/Comments/Delete`);
        request.send(data);
    })
}