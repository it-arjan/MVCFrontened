function getPartial(url, resultDivId) {
    $.ajax({
        type: 'Get',
        dataType: 'html',
        url: url,
    })
    .done(function (htmlPartial) {
        $(resultDivId).html(htmlPartial);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        $(resultDivId).html("<br/>Request failed: error= " + errorThrown);
    })
}
