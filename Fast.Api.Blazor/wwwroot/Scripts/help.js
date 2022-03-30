window.methods = {
    titleClick: function (id) {
        if ($("#" + id).css("display") == "none")
            $("#" + id).show();
        else
            $("#" + id).hide();

        $("div .title").each(function () {
            if ($(this).data("id") != id)
                $("#" + $(this).data("id")).hide();
        });
    },
    paramterClick: function (id) {
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
    submitClick: function (id,result,url) {
        $.ajax({
            type: "POST",
            timeout: 1000 * 60,
            url: url,
            data: $("#" + id + " input").serialize(),
            success: function (data) {
                $("#" + result).val(data);
            }
        });
    }
}