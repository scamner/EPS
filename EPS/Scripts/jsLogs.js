$(document).ready(function () {
    $("#tabs_Logs li").click(function () {
        TabPressed_Logs($(this));
    });

    SetTab_Logs('');
});

function TabPressed_Logs(tab) {
    $(document).attr("title", "EPS - " + tab.text());
    SetTab_Logs(tab);
}

function SetTab_Logs(tab) {
    var id = tab.attr('id');

    $("#tabs_Logs li").removeClass('active');
    tab.addClass("active");
    $(".tab_content_Logs").hide();
    var selected_tab = tab.find("a").attr("href");

    $('#divMainLogs').fadeIn("300", function () {
        LoadLogs(selected_tab.replace('#', ''));
    });
}

function LoadLogs(item) {
    $.ajaxSetup({
        cache: false
    });

    $("#divMainLogs").load('Dashboard/' + item);
}