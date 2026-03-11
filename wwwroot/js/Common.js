function showLoader() {
    $("#globalLoader").removeClass("d-none");
}

function hideLoader() {
    $("#globalLoader").addClass("d-none");
}

function showToast(message, isSuccess = true) {
    var $toast = $("#statusToast");
    var $toastBody = $("#statusMessage");

    // Set message
    $toastBody.text(message);

    // Change background color
    $toast.removeClass("bg-success bg-danger")
        .addClass(isSuccess ? "bg-success" : "bg-danger");

    // Show toast using Bootstrap
    var toast = new bootstrap.Toast($toast[0]);
    toast.show();
}

function file_download(url, data) {
    debugger
    var $iframe,
        iframe_doc,
        iframe_html;
    $('#download_iframe').remove();
    if (($iframe = $('#download_iframe')).length === 0) {
        $iframe = $("<iframe id='download_iframe'" +
            " style='display: none' src='about:blank'></iframe>"
        ).appendTo("body");
    }
    iframe_doc = $iframe[0].contentWindow || $iframe[0].contentDocument;
    if (iframe_doc.document) {
        iframe_doc = iframe_doc.document;
    }
    iframe_html = "<html><head></head><body><form method='POST' action='" +
        url + "'>";
    Object.keys(data).forEach(function (key) {
        iframe_html += "<input type='hidden' name='" + key + "' value='" + data[key] + "'>";
    });
    iframe_html += "</form></body></html>";
    iframe_doc.open();
    iframe_doc.write(iframe_html);
    $(iframe_doc).find('form').submit();
}
