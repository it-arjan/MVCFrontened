function CallFuncWhenAxjaxTokenValid(func, ajaxAccessToken, resultDivId, rowId) {
    
    $.ajax({
        type: 'GET',
        url: '/Message/AuthPing',
        beforeSend: function (xhr) {
            xhr.setRequestHeader('Authorization', 'bearer ' + ajaxAccessToken);
        },
    })
    .done(function (data) {
        $(resultDivId).append("OAUTH2 token check succeeded, proceeding to fetch your data..");
        if (typeof rowId === "undefined") func(ajaxAccessToken, resultDivId);
        else func(ajaxAccessToken, resultDivId, rowId);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        $(resultDivId).text("OAUTH2 Silicon Token expired, refresh the page.");
    });
}