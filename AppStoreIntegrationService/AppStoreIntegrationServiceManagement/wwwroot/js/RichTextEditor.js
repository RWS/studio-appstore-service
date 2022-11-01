let optionButtons = document.querySelectorAll('.rich-text-editor .option');
let alignButtons = document.querySelectorAll('.rich-text-editor .align');
let formatButtons = document.querySelectorAll('.rich-text-editor .format');
let editor = document.querySelector('.rich-text-editor');
let savedRange;

optionButtons.forEach(button => {
    button.addEventListener('click', () => {
        RestoreSel();
        document.execCommand(button.id, false, null);
    })
})

document.querySelector('.edit-area').addEventListener('focusout', () => {
    SaveSel();
});

document.addEventListener('selectionchange', () => {
    HighlighterRemover(optionButtons)

    if (!editor.contains(window.getSelection().anchorNode.parentNode)) {
        return false;
    }

    ParentTagActive(window.getSelection().anchorNode.parentNode);
})

function Highlighter(buttons, needsRemoval) {
    buttons.forEach(button => {
        button.addEventListener("click", () => {
            if (needsRemoval) {
                let alreadyActive = false;
                if (button.classList.contains("active")) {
                    alreadyActive = true;
                }
                HighlighterRemover(buttons);
                if (!alreadyActive) {
                    button.classList.add("active");
                }
            } else {
                button.classList.toggle("active");
            }
        });
    });
}

function Init() {
    Highlighter(alignButtons, true);
    Highlighter(formatButtons, false);
}

function HighlighterRemover(buttons) {
    buttons.forEach((button) => {
        button.classList.remove("active");
    });
}

function ParentTagActive(elem) {
    if (!elem || !elem.classList || elem.classList.contains('edit-area')) {
        return false;
    }

    let tagName = elem.tagName;
    let toolbarButton = document.querySelector(`.rich-text-editor i[data-tag-name="${tagName}"]`);
    if (toolbarButton) {
        toolbarButton.classList.add('active');
    }

    return ParentTagActive(elem.parentNode);
}

function SaveSel() {
    if (window.getSelection)//non IE Browsers
    {
        savedRange = window.getSelection().getRangeAt(0);
    }
    else if (document.selection)//IE
    {
        savedRange = document.selection.createRange();
    }
}

function RestoreSel() {
    $('.edit-area').focus();
    if (savedRange != null) {
        if (window.getSelection)//non IE and there is already a selection
        {
            var s = window.getSelection();
            if (s.rangeCount > 0)
                s.removeAllRanges();
            s.addRange(savedRange);
        }
        else if (document.createRange)//non IE and no selection
        {
            window.getSelection().addRange(savedRange);
        }
        else if (document.selection)//IE
        {
            savedRange.select();
        }
    }
}

function InsertLink() {
    RestoreSel();
    let placeHolder = document.getElementById('linkPlaceholder').value;
    let linkUrl = document.getElementById('link').value;
    document.execCommand('insertHTML', false, '<a href="' + linkUrl + '" target="_blank">' + (placeHolder == '' ? linkUrl : placeHolder) + '</a>');
    document.getElementById('linkPlaceholder').value = '';
    document.getElementById('link').value = ''
}

window.onload = Init();