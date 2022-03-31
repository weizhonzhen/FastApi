var editor;
window.methods = {
    Help_TitleClick: function (id) {
        if ($("#" + id).css("display") == "none")
            $("#" + id).show();
        else
            $("#" + id).hide();

        $("div .title").each(function () {
            if ($(this).data("id") != id)
                $("#" + $(this).data("id")).hide();
        });
    },
    Help_ParamterClick: function (id) {
        debugger;
        $("div input").each(function () {
            $(this).val("");
        });
        $("div textarea").each(function () {
            $(this).val("");
        });

        if ($("#" + id).css("display") == "none")
            $("#" + id).show();
        else
            $("#" + id).hide();

        $("table").each(function () {
            if ($(this).attr("id") != id)
                $(this).hide();
        });
    },
    Help_SubmitClick: function (id, result, url) {
        $.ajax({
            type: "POST",
            timeout: 1000 * 60,
            url: url,
            data: $("#" + id + " input").serialize(),
            success: function (data) {
                $("#" + result).val(data);
            }
        });
    },
    Xml_TitleClick: function () {
        $("table tr").first().click()
        $("table tbody tr").click(function () {
            $(this).css("background-color", "#6CC2CC");
            $("table tbody tr:odd").not(this).css("background-color", "#ffffff");
            $("table tbody tr:even").not(this).css("background-color", "#f3f4f5");
            $("textarea").val($(this).children().data("value"));
            $("#name").val($(this).children().data("name"));
            $(".CodeMirror").remove();
            editor = CodeMirror.fromTextArea(document.getElementById("code"), { mode: "text/xml", lineNumbers: true });
        });
    },
    Xml_Save: function (id) {
        DotNet.invokeMethodAsync('Fast.Api.Blazor', 'XmlSaveAsyn', $("#name").val(), editor.getValue()).then(data => {
            if (data.Issuccess)
                window.location.reload();
            else
                alert(data.msg);
        });
    },
    Xml_Del: function () {
        debugger;
        DotNet.invokeMethodAsync('Fast.Api.Blazor', 'XmlDelAsyn', $("#name").val()).then(data => {
            if (data.Issuccess)
                window.location.reload();
            else
                alert(data.msg);
        });
    },
    Xml_Add: function () {
        $(".CodeMirror").remove();
        $("textarea").show();
        $("textarea").val("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<sqlMap>\n\t<select id=\"\" db=\"\" type=\"\">\n\r\t</select>\n</sqlMap>");
        $("#name").val(".xml");
        editor = CodeMirror.fromTextArea(document.getElementById("code"), { mode: "text/xml", lineNumbers: true });
    }
}