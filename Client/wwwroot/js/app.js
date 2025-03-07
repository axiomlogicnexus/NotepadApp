// Prevent default behavior for events
function preventDefault(event) {
    event.preventDefault();
}

// Auto resize textarea
function autoResizeTextarea(textarea) {
    textarea.style.height = 'auto';
    textarea.style.height = (textarea.scrollHeight) + 'px';
}
