$(document).ready(function () {
    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        ToggleSidebarClass();
    }
});

function ToggleSidebar() {
    ToggleSidebarClass();

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        localStorage.setItem('ToggledSidebar', 'false');
    }
    else {
        localStorage.setItem('ToggledSidebar', 'true');
    }
}

function ToggleSidebarClass() {
    $('.custom-sidebar').toggleClass('toggled');
    $('.page_labels').toggleClass('toggled');
    $('.sidebar-collapse').toggleClass('toggled');
    $('.icon-arrow').toggleClass('toggled');
    $('.copyright').toggleClass('toggled');
    $('.loading').toggleClass('toggled');
    $('.icon-hideez').toggleClass('toggled');
}

function toggleModalDialog(dialogId) {
    $('#' + dialogId).modal('toggle');
}

function copyToClipboard() {
    $("#activationCodeInput").attr("type", "Text").select();
    document.execCommand("copy");
    $("#activationCodeInput").attr("type", "Password").select();
}

function downloadLog(filename, content) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(content)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function downloadPersonalData(content) {
    var link = document.createElement('a');
    link.download = "PersonalData.json";
    link.href = "data:text/json;charset=utf-8," + encodeURIComponent(content)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function generateQr(text) {
    new QRCode(document.getElementById("qrCode"),
        {
            text: text,
            width: 150,
            height: 150
        });
}

function setCookie(cookie) {
    document.cookie = cookie;
}

function removeCookie(cookieName) {
    document.cookie = cookieName + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}

function setFocus(elementId) {
    document.getElementById(elementId).focus();
}

function showModalDialog(dialogId) {
    let modal = new bootstrap.Modal(document.getElementById(dialogId), { backdrop: 'static', keyboard: false });
    modal.show();
}

function hideModalDialog(dialogId) {
    var modal = bootstrap.Modal.getInstance(document.getElementById(dialogId));
    modal.hide();
}