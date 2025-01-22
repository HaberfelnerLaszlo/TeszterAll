// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

 function addToRichText(text_to_add) {
     const textarea = document.getElementById("richText");
     let start_position = textarea.selectionStart;
     let end_position = textarea.selectionEnd;
     let select = textarea.value.substring(start_position, end_position);
     textarea.value = `${textarea.value.substring(0, start_position)}${text_to_add}${textarea.value.substring(end_position, textarea.value.length
     )}`;
     return select;
};
function wrapText(startTag, endTag) {
    const textarea = document.getElementById('richText');

    if (window.getSelection) {
        sel = window.getSelection();
        let selectedText = sel.toString();
        if (sel.rangeCount > 0) {
            containerNode = sel.getRangeAt(0).commonAncestorContainer;
        }
    } else if ((sel = document.selection) && sel.type != "Control") {
        containerNode = sel.createRange().parentElement();
    }
    //const start = textarea.innerText.selectionStart;
    //const end = textarea.innerHtml.selectionEnd;
    //const text = textarea.value;

    //// Get the selected text
    //const selectedText = text.substring(start, end);

    //// Replace the selected text with formatted text
    //const formattedText = startTag + selectedText + endTag;
    //textarea.value = text.substring(0, start) + formattedText + text.substring(end);

    //// Reposition the cursor
    //textarea.selectionStart = start;
    //textarea.selectionEnd = start + formattedText.length;

    // Focus back on the textarea
    textarea.focus();
}

window.getDimensions = {
    win: function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    },
    comp: function (element) {
        var el = document.getElementById(element)
        if (el == null) {
            return {
                width: -1,
                height: -1
            }
        }
        return {
            width: el.clientWidth,
            height: el.clientHeight
        }
    }
};