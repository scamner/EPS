$(document).ready(function () {
    $("#tabs li").click(function () {
        TabPressed($(this));
    });
});

function TabPressed(tab) {
    $(document).attr("title", "EPS - " + tab.text());

    if (tab.text() !== "Log Out") {
        SetTab(tab);
    }
}

function SetTab(tab) {
    var id = tab.attr('id');
    var param = '';

    if (id !== "GetRuns") {
        timerRunCheck = "off";
        clearTimeout(timerRunCheck);

        $("#divMain").html('');
    }
    else {

        param = "?FutureRuns=" + $('#chkFutureRuns').is(':checked');
    }

    $("#tabs li").removeClass('active');
    tab.addClass("active");
    $(".tab_content").hide();
    var selected_tab = tab.find("a").attr("href");
    $('#divMain').fadeIn("300", function () {
        LoadMain(selected_tab.replace('#', '') + param);
    });
}

function LoadMain(item) {
    $.ajaxSetup({
        cache: false
    });

    $("#divMain").load('Dashboard/' + item);   
}

function checkRuns() {
    if (timerRunCheck !== "off") {
        var tab = $('#GetRuns');
        SetTab(tab);
    }
}