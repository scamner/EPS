$(document).ready(function () {
    $("#tabs_logs li").click(function () {
        TabPressed_Logs($(this));
    });

    SetTab_Logs($('#GetEmployeesLog'));
});

function TabPressed_Logs(tab) {
    $(document).attr("title", "EPS - " + tab.text());
    SetTab_Logs(tab);
}

function SetTab_Logs(tab) {
    var id = tab.attr('id');

    $("#tabs_logs li").removeClass('active');
    tab.addClass("active");
    $(".tab_content_logs").hide();
    var selected_tab = tab.find("a").attr("href");

    $('#divMainLogs').fadeIn("300", function () {
        LoadLogs(selected_tab.replace('#', ''));
    });
}

function LoadLogs(item) {
    $.ajaxSetup({
        cache: false
    });

    $("#divMainLogs").html('');
    $("#divMainLogs").load('Dashboard/' + item);
}

function FilterEmployeeLog(sortOrder, sortDir) {
    var un = $('#Username_Filter').val();
    var fn = $('#FirstName_Filter').val();
    var ln = $('#LastName_Filter').val();
    var page = $('#hidPageNum').val();
    var user = $('#UserID_Filter').val();
    var dateFrom = $('#AuditDateFrom_Filter').val();
    var dateTo = $('#AuditDateTo_Filter').val();
    var type = $('#selUser_Filter').val();

    sortOrder = sortOrder === undefined ? '' : sortOrder;
    sortDir = sortDir === undefined ? '' : sortDir;

    LoadLogs('GetEmployeesLog?sortOrder=' + sortOrder + '&SortDirection=' + sortDir + '&page=' + page + '&Username=' + encodeURIComponent(un) + '&FirstName=' + encodeURIComponent(fn) + '&LastName=' + encodeURIComponent(ln) +
        '&AuditUser=' + user + '&AuditDateFrom=' + dateFrom + '&AuditDateTo=' + dateTo + '&ChangeType=' + type);
}

function FilterWorkflowRunLog(sortOrder, sortDir) {
    var iid = $('#ItemID_Filter').val();
    var fn = $('#FirstName_Filter').val();
    var ln = $('#LastName_Filter').val();
    var rid = $('#ResultID_Filter').val();
    var wfid = $('#WorkflowID_Filter').val();
    var page = $('#hidPageNum').val();
    var user = $('#UserID_Filter').val();
    var dateFrom = $('#AuditDateFrom_Filter').val();
    var dateTo = $('#AuditDateTo_Filter').val();
    var type = $('#selUser_Filter').val();

    sortOrder = sortOrder === undefined ? '' : sortOrder;
    sortDir = sortDir === undefined ? '' : sortDir;

    LoadLogs('GetWorkflowRunLog?sortOrder=' + sortOrder + '&SortDirection=' + sortDir + '&page=' + page + '&ItemID=' + iid + '&FirstName=' + encodeURIComponent(fn) + '&LastName=' + encodeURIComponent(ln) +
        '&ResultID=' + rid + '&WorkflowID=' + wfid +
        '&AuditUser=' + user + '&AuditDateFrom=' + dateFrom + '&AuditDateTo=' + dateTo + '&ChangeType=' + type);
}

function FilterWorkflowLog(sortOrder, sortDir) {
    var it = $('#ItemType_Filter').val();
    var iname = $('#ItemName_Filter').val();
    var page = $('#hidPageNum').val();
    var user = $('#UserID_Filter').val();
    var dateFrom = $('#AuditDateFrom_Filter').val();
    var dateTo = $('#AuditDateTo_Filter').val();
    var type = $('#selUser_Filter').val();

    sortOrder = sortOrder === undefined ? '' : sortOrder;
    sortDir = sortDir === undefined ? '' : sortDir;

    LoadLogs('GetWorkflowLog?sortOrder=' + sortOrder + '&SortDirection=' + sortDir + '&page=' + page + '&ItemType=' + encodeURIComponent(it) + '&ItemName=' + encodeURIComponent(iname) +
        '&AuditUser=' + user + '&AuditDateFrom=' + dateFrom + '&AuditDateTo=' + dateTo + '&ChangeType=' + type);
}

function FilterParametersLog(sortOrder, sortDir) {
    var pn = $('#ParamName_Filter').val();
    var page = $('#hidPageNum').val();
    var user = $('#UserID_Filter').val();
    var dateFrom = $('#AuditDateFrom_Filter').val();
    var dateTo = $('#AuditDateTo_Filter').val();
    var type = $('#selUser_Filter').val();

    sortOrder = sortOrder === undefined ? '' : sortOrder;
    sortDir = sortDir === undefined ? '' : sortDir;

    LoadLogs('GetParametersLog?sortOrder=' + sortOrder + '&SortDirection=' + sortDir + '&page=' + page + '&ParamName=' + encodeURIComponent(pn) + 
        '&AuditUser=' + user + '&AuditDateFrom=' + dateFrom + '&AuditDateTo=' + dateTo + '&ChangeType=' + type);
}

function FilterUsersLog(sortOrder, sortDir) {
    var un = $('#Username_Filter').val();
    var fn = $('#FirstName_Filter').val();
    var ln = $('#LastName_Filter').val();
    var page = $('#hidPageNum').val();
    var user = $('#UserID_Filter').val();
    var dateFrom = $('#AuditDateFrom_Filter').val();
    var dateTo = $('#AuditDateTo_Filter').val();
    var type = $('#selUser_Filter').val();

    sortOrder = sortOrder === undefined ? '' : sortOrder;
    sortDir = sortDir === undefined ? '' : sortDir;

    LoadLogs('GetUsersLog?sortOrder=' + sortOrder + '&SortDirection=' + sortDir + '&page=' + page + '&Username=' + encodeURIComponent(un) + '&FirstName=' + encodeURIComponent(fn) + '&LastName=' + encodeURIComponent(ln) +
        '&AuditUser=' + user + '&AuditDateFrom=' + dateFrom + '&AuditDateTo=' + dateTo + '&ChangeType=' + type);
}