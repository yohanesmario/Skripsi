var inputs = document.getElementsByTagName("input");
var ahrefs = document.getElementsByTagName("a");
var buttons = document.getElementsByTagName("button");

function addClickListener(el, tagName, idx) {
    el.addEventListener("click", function () {
        ScriptNotifyHandler.passAction("document.getElementsByTagName(\"" + tagName + "\")[" + idx + "].click();");
    });
}
function addChangeListener(el, tagName, idx) {
    el.addEventListener("change", function () {
        ScriptNotifyHandler.passAction("document.getElementsByTagName(\"" + tagName + "\")[" + idx + "].value = " + JSON.stringify(document.getElementsByTagName(tagName)[idx].value) + ";");
    });
}

for (var i = 0; i < inputs.length; i++) {
    addClickListener(inputs[i], "input", i);
    addChangeListener(inputs[i], "input", i);
}

for (var i = 0; i < ahrefs.length; i++) {
    addClickListener(ahrefs[i], "a", i);
}

for (var i = 0; i < buttons.length; i++) {
    addClickListener(buttons[i], "button", i);
}