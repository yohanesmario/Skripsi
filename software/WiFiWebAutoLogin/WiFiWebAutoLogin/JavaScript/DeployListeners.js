function addClickListener(el, tagName, i) {
    el.addEventListener("click", function () {
        ScriptNotifyHandler.notify("document.getElementsByTagName(\"" + tagName + "\")[" + i + "].click();");
    });
}
function addChangeListener(el, tagName, i) {
    el.addEventListener("change", function () {
        ScriptNotifyHandler.notify("document.getElementsByTagName(\"" + tagName + "\")[" + i + "].value = " + JSON.stringify(document.getElementsByTagName(tagName)[i].value) + ";");
    });
}

var inputs = document.getElementsByTagName("input");
for (var i = 0; i < inputs.length; i++) {
    addClickListener(inputs[i], "input", i);
    addChangeListener(inputs[i], "input", i);
}

var ahrefs = document.getElementsByTagName("a");
for (var i = 0; i < ahrefs.length; i++) {
    addClickListener(ahrefs[i], "a", i);
}

var buttons = document.getElementsByTagName("button");
for (var i = 0; i < buttons.length; i++) {
    addClickListener(buttons[i], "button", i);
}