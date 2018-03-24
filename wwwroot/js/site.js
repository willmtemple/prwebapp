// Write your JavaScript code.

$('.ui.radio.checkbox').checkbox();

var mdc = new showdown.Converter();
$("div.mdfmt").html(function(_, t) { return mdc.makeHtml(t); });