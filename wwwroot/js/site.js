// Write your JavaScript code.

var mdc = new showdown.Converter();
$("div.mdfmt").html(function(_, t) { return mdc.makeHtml(t); });