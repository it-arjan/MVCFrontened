var allBoxes = ["#message_box1", "#message_box2", "#message_box3"];
var busyBoxes = [];

function DisplayNotification(notification) {
    var finder = new FreeBoxFinder();
    finder.findBox(finder, notification);
    if (!finder.found) {
        //console.log("trying again..");
        //
    }
}

function FreeBoxFinder() {
    this.found = false,
        this.findBox = FreeBoxFinderFunc
}

function FreeBoxFinderFunc(caller, notification) {
    for (var i = 0; i < allBoxes.length; i++) {
        if (busyBoxes.indexOf(allBoxes[i]) < 0) {
            //console.log("animating " + allBoxes[i]);
            caller.found = true;
            AnimateMessageBox(allBoxes[i], notification);
            break;
        }
    }
    if (!caller.found) {
        console.log("no free box available, trying again in a sec or so");
        setTimeout(function () {
            caller.findBox(caller, notification);
        }, 1000);
    }
}

function AnimateMessageBox(cssId, notification) {
    //console.log("starting animation for box " + cssId);
    busyBoxes.push(cssId);
    $(cssId).css({
        visibility: "visible",
        top: 150
    }).text(notification.toString())
        .animate({
            top: 0
        }, 8500, function () {
            var idx = busyBoxes.indexOf(cssId);
            //console.log("removing " + cssId + " from pos " + idx);
            busyBoxes.splice(idx, 1);
            $(cssId).css({
                visibility: "hidden"
            })
        });
}