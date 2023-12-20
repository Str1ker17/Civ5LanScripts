function processResults(data) {
    if (data === undefined) {
        $("#latency-indicator")[0].innerText = "Ошибка сервера!!"
        return;
    }

    $("#src_ip")[0].innerText = data.src_ip;
    ping = data.latency_min;
    jitter = data.latency_max - data.latency_min;

    if (ping > 0) {
        $("#latency-indicator")[0].innerText = ping + " мс."
    }

    if (ping < 0) {
        latency_eval = "none";
    } else if (ping < 10) {
        latency_eval = "excellent";
    } else if (ping < 50) {
        latency_eval = "good";
    } else if (ping < 100) {
        latency_eval = "fair";
    } else {
        latency_eval = "bad";
    }
    $("#latency-" + latency_eval).show();

    if (jitter > 0) {
        $("#jitter-indicator").text(ping + " мс.");
        if (jitter > 20) {
            $("#jitter-significant").show();
        }
    }
}

function doConnectionTest_Complete(data) {
    processResults(data.responseJSON);
    if (data.latency_min !== undefined && data.latency_min >= 0) {
        callRPC("latency", $("#jitter-indicator"));
    }
}

function callRPC(name, obj) {
    $.ajax({
        url: "api.php",
        method: "POST",
        data: JSON.stringify({ "func": name }),
        contentType: "application/json", /* type of POST body */
        dataType: "json", /* jQuery converter */
        mimeType: "application/json", /* override server response type */
        beforeSend: function() { obj.find(".loading.circle").show(); },
        complete: function(data) { obj.find(".loading.circle").hide(); doConnectionTest_Complete(data); }
    });
}

function doConnectionTest() {
    callRPC("ping", $("#latency-indicator"));
}

(function init($, undefined) {
    $(".loading").hide();
    $(".loading.circle").html("<img src='loading.gif'/>");
    $(".loading.bar").html("<img src='loading_bar.gif'/>");
    $(document).on("ajaxStart", function() { $(".loading.bar").show(); });
    $(document).on("ajaxStop", function() { $(".loading.bar").hide(); });
    doConnectionTest();
})(jQuery);
