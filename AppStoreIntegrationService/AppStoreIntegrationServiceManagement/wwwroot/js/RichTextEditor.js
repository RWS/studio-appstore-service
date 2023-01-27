let optionButtons;
let alignButtons;
let formatButtons;
let selectorCells;
let editor;
let savedRange;

function HighlightSelectionCells(rows, cols) {
    selectorCells.forEach(selector => {
        let [selRow, selCol] = [parseInt(selector.id.split('x')[0], 10), parseInt(selector.id.split('x')[1], 10)];
        if (selRow <= rows && selCol <= cols) {
            selector.style.backgroundColor = "lightblue";
        }
    })
}

function InsertTable(rows, cols) {
    var table = document.createElement('table');
    for (let i = 0; i < rows; i++) {
        let row = document.createElement('tr');
        for (let j = 0; j < cols; j++) {
            let col = document.createElement('td');
            col.style.height = "40px";
            col.style.border = "1px solid black"
            col.style.padding = "8px"
            row.append(col);
        }
        table.append(row);
    }

    RestoreSel();
    table.style.border = "1px solid black";
    table.style.tableLayout = "fixed"
    table.classList.add('table');
    document.querySelector('.edit-area').appendChild(table);
}

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
    optionButtons = document.querySelectorAll('.rich-text-editor .option');
    alignButtons = document.querySelectorAll('.rich-text-editor .align');
    formatButtons = document.querySelectorAll('.rich-text-editor .format');
    selectorCells = document.querySelectorAll('.rich-text-editor .selector-cell');
    editor = document.querySelector('.rich-text-editor');

    selectorCells.forEach(selector => {
        selector.addEventListener('mouseover', () => {
            document.querySelector('.table-dimension').innerText = selector.id;
            let [rows, cols] = [parseInt(selector.id.split('x')[0], 10), parseInt(selector.id.split('x')[1], 10)];
            HighlightSelectionCells(rows, cols);
        })

        selector.addEventListener('click', () => {
            let [rows, cols] = [parseInt(selector.id.split('x')[0], 10), parseInt(selector.id.split('x')[1], 10)];
            InsertTable(rows, cols);
        })
    })

    let tableSelector = document.querySelector('.table-dimension-selector')
    if (tableSelector) {
        tableSelector.addEventListener('mouseout', () => {
            document.querySelector('.table-dimension').innerText = "0x0";
            selectorCells.forEach(selector => {
                selector.style.backgroundColor = "white";
            })
        })
    }

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
        if (window.getSelection().anchorNode.parentNode || !editor.contains(window.getSelection().anchorNode.parentNode)) {
            return false;
        }

        ParentTagActive(window.getSelection().anchorNode.parentNode);
    })

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
    $('.edit-area').focus((e) => {
        e.preventDefault();
        e.target.focus({ preventScroll: true });
    });

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

function ShowInsertionModal() {
    $('#linkModal').modal('show');
    RestoreSel();
    document.querySelector('#linkModal #linkPlaceholder').value = savedRange.startContainer.data.substring(savedRange.startOffset, savedRange.endOffset);
}

function InsertLink() {
    RestoreSel();
    let placeHolder = document.querySelector('#linkModal #linkPlaceholder').value;
    let image = document.querySelector('#linkModal #imageLink').value;
    let linkUrl = document.querySelector('#linkModal #link').value;

    let tag;
    switch (true) {
        case linkUrl == '': return;
            break;
        case image == '' && placeHolder == '': tag = '<a href="' + linkUrl + '" target="_blank">' + linkUrl + '</a>';
            break;
        case image != '' && placeHolder != '': tag = '<a href="' + linkUrl + '" target="_blank"><img width="200" heigth="200" src="' + image + '" title="' + placeHolder + '" alt="Image for' + placeHolder + ' link"></a>';
            break;
        case image == '' && placeHolder != '': tag = '<a href="' + linkUrl + '" target="_blank">' + placeHolder + '</a>';
            break;
        default: tag = '<a href="' + linkUrl + '" target="_blank"><img width="200" heigth="200" src="' + image + '"></a>';
            break;
    }

    document.execCommand('insertHTML', true, tag);
    document.querySelector('#linkModal #linkPlaceholder').value = '';
    document.querySelector('#linkModal #imageLink').value = '';
    document.querySelector('#linkModal #link').value = ''
}