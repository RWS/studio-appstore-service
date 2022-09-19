function Collapse(element) {

    if (window.innerWidth > 576) {
        return;
    }

    var children = element.closest('tr').children;
    for (let child of children) {
        if (child != element.parentElement) {
            child.classList.toggle('d-none');
        }
    }
}