function SendCmd(url, formData, ajaxAccessToken, resultDivId) {
    $.ajax({
        type: 'POST',
        url: url,
        data: formData,
        dataType: 'text',
        encode: true,
        beforeSend: function (xhr) {
            xhr.setRequestHeader('Authorization', 'bearer ' + ajaxAccessToken);
            xhr.setRequestHeader('Access-Control-Allow-Origin', '*');
        },
    })
        .done(function (data) {
            var obj = JSON.parse(data);
            $(resultDivId).text(obj.Message);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            var msg = "";
            if (jqXHR.status == 401) {
                msg = "AUTHENTICATION FAILED, is the Cors token expired?"
            }
            else {
                msg = "Sending command failed, status= " + jqXHR.status
            }
            $(resultDivId).text(msg);
        })
}