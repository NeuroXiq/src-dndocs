export function setInnerHTMLById(elementId, innerHTML) {
    document.getElementById(elementId).innerHTML = innerHTML;
}

export function getById(elementId) {
    return document.getElementById(elementId);
}

export function isHiddenById(elementId, shouldHide) {
    isHidden(document.getElementById(elementId), shouldHide);
}

export function isHidden(element, shouldHide) {
    if (shouldHide) {
        element.classList.add('is-hidden');
    }
    else {
        element.classList.remove('is-hidden');
    }
}