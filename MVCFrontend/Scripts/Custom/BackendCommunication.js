"use strict";
function CallFuncWhenCookieStillValid(func, ajaxAccessToken, resultDivId, funcData) {

    $.ajax({
        type: 'GET',
        url: '/Message/AuthPing',
        //beforeSend: function (xhr) {
        //    xhr.setRequestHeader('Authorization', 'bearer ' + ajaxAccessToken);
        //},
    })
        .done(function (authPingResult) {
            if (typeof funcData === "undefined") func(ajaxAccessToken, resultDivId);
            else func(ajaxAccessToken, resultDivId, funcData);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $(resultDivId).text("Auth cookie expired, refresh the page.");
        });
}

function deletePostback(id, resultDivId) {
    CallFuncWhenCookieStillValid(DeletePostbackAjax, getRequestVeirificationToken(), resultDivId, id);
}
function openPopupDetails(postbackId, resultDivId) {
    $(resultDivId).slideToggle();
    CallFuncWhenCookieStillValid(QueryDetails, 'token-not-used', resultDivId, postbackId);
}

function getRequestVeirificationToken() {
    return $("input[name='__RequestVerificationToken']").attr("value");
}

function DeletePostbackAjax(requestVerificationToken, resultDivId, id) {
    var formData = {
        __RequestVerificationToken: requestVerificationToken
    };
    $.ajax({
        type: 'Post',
        dataType: 'html',
        url: '/Postbackdatas/Delete/' + id,
        data: formData
    })
        .done(function (data) {
            $(resultDivId).html(data);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $(resultDivId).append("<br/>Request failed: error= " + errorThrown);
        })
}

function QueryDetails(ajaxAccessToken, resultDivId, postbackId) {

    $(resultDivId).html("<h3>Loading details ... </h3>");
    getPartial('/Postbackdatas/Details?id=' + postbackId, resultDivId);
}

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
