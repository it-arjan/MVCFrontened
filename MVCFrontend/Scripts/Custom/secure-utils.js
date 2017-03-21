function CallFuncWhenAxjaxTokenValid(func, ajaxAccessToken, resultDivId, funcData) {
    
    $.ajax({
        type: 'GET',
        url: '/Message/AuthPing',
        beforeSend: function (xhr) {
            xhr.setRequestHeader('Authorization', 'bearer ' + ajaxAccessToken);
        },
    })
    .done(function (authPingResult) {
        $(resultDivId).append("OAUTH2 token check succeeded, proceeding to fetch your data..");
        if (typeof funcData === "undefined") func(ajaxAccessToken, resultDivId);
        else func(ajaxAccessToken, resultDivId, funcData);
    })
    .fail(function (jqXHR, textStatus, errorThrown) {
        $(resultDivId).text("OAUTH2 Silicon Token expired, refresh the page.");
    });
}