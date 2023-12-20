var measurements_done = false;

function processResults(data) {
    if (data === undefined || data.status === undefined || data.src_ip === undefined) {
        $("#latency-indicator").text("Неизвестная ошибка сервера!!");
        return false;
    }
    if (data.status != 0) {
        $("#latency-indicator").text(data.status + " - " + data.msg);
        return false;
    }

    $("#src_ip").text(data.src_ip);

    ping = data.latency_min;
    if (ping < 0) {
        $("#latency-eval-none").show();
        return;
    }
    $("#latency-indicator").text(ping + " мс.");

    jitter = data.latency_max - data.latency_min;
    if (jitter <= 0) { /* Only one ping is not precise to make assumptions. */
        return;
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
    $("#latency-eval-" + latency_eval).show();
    
    $("#jitter-indicator").text(jitter + " мс.");
    if (jitter > 25) {
        $("#jitter-significant").show();
    }
    $("#technical_data").text(JSON.stringify(data));
}

function doConnectionTest_Complete(data) {
    res = data.responseJSON;
    processResults(res);
    if (!measurements_done && res.latency_min !== undefined && res.latency_min >= 0) {
        callRPC("latency", $("#jitter-indicator"));
        measurements_done = true;
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
    $(".loading.circle").html("<img src='loading.gif' width='16' height='16'/>");
    $(document).on("ajaxStart", function() { $(".loading.bar").show(); });
    $(document).on("ajaxStop", function() { $(".loading.bar").hide(); });
    doConnectionTest();
})(jQuery);
