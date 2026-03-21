export function setInnerHTMLById(elementId, innerHTML) {
    document.getElementById(elementId).innerHTML = innerHTML;
}

export function getElById(elementId) {
    return document.getElementById(elementId);
}

export function toggleHideById(elementId, isHidden) {
    toggleHide(document.getElementById(elementId), isHidden);
}

export function toggleHide(element, isHidden) {
    if (isHidden) {
        element.classList.add('is-hidden');
    }
    else {
        element.classList.remove('is-hidden');
    }
}