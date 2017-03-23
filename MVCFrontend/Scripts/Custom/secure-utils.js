function CallFuncWhenAxjaxTokenValid(func, ajaxAccessToken, resultDivId, funcData) {
    
    $.ajax({
        type: 'GET',
        url: '/Message/AuthPing',
        beforeSend: function (xhr) {
            xhr.setRequestHeader('Authorization', 'bearer ' + ajaxAccessToken);
        },
    })
    .done(function (authPingResult) {
        if (typeof funcData === "undefined") func(ajaxAccessToken, resultDivId);
        else func(ajaxAccessToken, resultDivId, funcData);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        $(resultDivId).text("OAUTH2 Silicon Token expired, refresh the page.");
    });
}