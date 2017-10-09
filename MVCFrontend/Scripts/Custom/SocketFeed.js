"use strict";

function createSocketFeed(socket_msg, qm_feed_id, notification_feed_id, api_feed_id, doneToken) {
    if (socket_msg.data.toString().indexOf(qm_feed_id.toString()) === 0) {
        return new QmFeed(qm_feed_id, doneToken);
    }
    if (socket_msg.data.toString().indexOf(api_feed_id.toString()) === 0) {
        return new ApiFeed(qm_feed_id);
    }
    if (socket_msg.data.toString().indexOf(notification_feed_id.toString()) === 0) {
        return new NotificationFeed(qm_feed_id);
    }
}
function SocketFeed() {
    // dummy required to make static function work .. duh
}
SocketFeed.StripId = function (socket_msg, feed_id) {
    return socket_msg.data.toString().substring(feed_id.toString().length + 6, socket_msg.data.toString().length);
}

function QmFeed(feedId, doneToken) {
    this.feedId = feedId;
    this.doneToken = doneToken;

    this.HandleMessage = function (socket_msg) {
        var bare_msg = SocketFeed.StripId(socket_msg, this.feedId);
        if (bare_msg.indexOf(this.doneToken.toString()) >= 0) {
            NotificationFeed.Show("Done token received, auto-refreshing postbacks...");
            CallFuncWhenCookieStillValid(QueryPostbackData, 'token-not-used', "#postbackResult");
        }
        else {
            QmFeed.Show(bare_msg);
        }

    }
}
QmFeed.Show = function (msg) {
    $("#socketChat").prepend("<p>" + msg + "</p>");
}

function ApiFeed(feedId) {
    this.feedId = feedId;
    this.HandleMessage = function (socket_msg) {
        ApiFeed.Show(SocketFeed.StripId(socket_msg, this.feedId));
    }
}
ApiFeed.Show = function (msg_data) {
    if ($("#api-request-feed").text().startsWith("Data Api")) {
        $("#api-request-feed").html("<p>" + msg_data + "</p>");
    }
    else {
        $("#api-request-feed").prepend("<p>" + msg_data + "</p>");
    }
}

function NotificationFeed(feedId) {
    this.feedId = feedId;
    this.HandleMessage = function (socket_msg) {
        NotificationFeed.Show(SocketFeed.StripId(socket_msg, this.feedId));
    }
}
NotificationFeed.Show = function (msg) {
    var box = new NotificationBox(msg);
    box.Animate(320, 400, 100);
}

function NotificationBox(Text) {
    this.Id = 'id-' + Math.random().toString(36).substr(2, 16);
    this.Text = Text;

    this.getRandomInt = function (min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    };

    this.Animate = function (boxWith, minTop, minLeft) {
        var tmpDiv = document.createElement("div");
        tmpDiv.setAttribute("id", this.CssId);
        tmpDiv.setAttribute("class", "message_box");
        document.body.appendChild(tmpDiv);

        var topPos = this.getRandomInt(minTop, window.innerHeight);
        var leftpos = this.getRandomInt(minLeft, window.innerWidth - boxWith);

        $(tmpDiv).css({
            position: "absolute",
            left: leftpos,
            top: topPos
        }).text(this.Text)
            .animate({
                top: 0
            }, 8500, function () {
                tmpDiv.parentElement.removeChild(tmpDiv);
            });
    }
}