window.addToTextArea = function (text_to_add) {
    const textarea = document.getElementById("qText");
    let start_position = textarea.control.selectionStart;
    let end_position = textarea.control.selectionEnd;
    let select = textarea.value.substring(start_position, end_position);
    textarea.value = `${textarea.value.substring(0,start_position)}${text_to_add}${textarea.value.substring(end_position, textarea.value.length
    )}`;
    return select;
};
