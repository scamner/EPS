function LoadSyncAD() {
    $('#FirstName').val('');
    $('#LastName').val('');

    $('#divSearchADUsersResult').html('');

    PopModal($('#divSyncADModal'), 'show', 'auto', true, false, 'FirstName');
}

function AddEmployee(ADGUID, Email, FirstName, LastName, Username, EmpNum) {
    var isManager = $('#IsManager_' + Username).is(':checked');
    var reportsTo = $('#ddlReportTo_' + Username).val();

    $.ajax({
        url: '/Dashboard/AddEmployee?ADGUID=' + ADGUID + '&Email=' + Email + '&FirstName=' + FirstName + '&LastName=' + LastName + '&Username=' + Username +
            '&IsManager=' + isManager + '&ManagerID=' + reportsTo + '&EmpNum=' + EmpNum,
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                LoadMain("GetEmployees");

                $('#lblAddEmp_' + Username).css('display', 'block');
                $('#btnAddEmp_' + Username).css('display', 'none');
                $('#ddlReportTo_' + Username).css('display', 'none');

                ShowMessage('Employee Added', 'show', false);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function SetEmployeeAsManager(empID, isManager) {
    $.ajax({
        url: '/Dashboard/SetEmployeeAsManager?EmpID=' + empID + '&IsManager=' + isManager,
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                ShowMessage('Employee Updated', 'show', false);

                if (isManager === false) {
                    $(".ManagerDropdown option[value='" + empID + "']").each(function () {
                        $(this).remove();
                    });
                }
                else {
                    $(".ManagerDropdown").each(function () {
                        $(this).append('<option value="' + empID + '">' + data.ManagerName + '</option>');

                        var $o = $(this).children().toArray();
                        $(this).append($o);
                    });
                }
            }
            else {
                ShowMessage(data.Error, 'show');

                var chk = $('#chsIsManager_' + empID);

                if (isManager === true) {
                    chk.prop("checked", false);
                }
                else {
                    chk.prop("checked", true);
                }
            }
        }
    });
}

function SetEmployeeReportTo(empID, userID) {
    $.ajax({
        url: '/Dashboard/SetEmployeeReportTo?EmpID=' + empID + '&ManagerID=' + userID,
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                ShowMessage('Employee Updated', 'show', false);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function FilterEmployees(sortOrder, sortDir) {
    var un = $('#Username_Filter').val();
    var fn = $('#FirstName_Filter').val();
    var ln = $('#LastName_Filter').val();
    var im = $('#IsManager_Filter').val();
    var mi = $('#ManagerID_Filter').val();
    var page = $('#hidPageNum').val();

    sortOrder = sortOrder === undefined ? '' : sortOrder;
    sortDir = sortDir === undefined ? '' : sortDir;

    LoadMain("GetEmployees" + '?sortOrder=' + sortOrder + '&SortDirection=' + sortDir + '&page=' + page + '&Username=' + un + '&FirstName=' + fn + '&LastName=' + ln +
        '&IsManager=' + im + '&ManagerID=' + mi);
}

var workflows;

function LoadRunWorkflow(empID) {
    $('#hidRunWorkflowEmpID').val(empID);

    $.ajax({
        url: '/Dashboard/LoadRunWorkflows',
        type: 'GET',
        datatype: 'json',
        cache: false,
        complete: function (data) {
            $('#selWorkflowsToRun').html('');

            var result = JSON.parse(data.responseText);

            if (result.Error === "") {
                workflows = JSON.parse(result.WorkFlows);

                $.each(workflows, function (i, w) {
                    $('#selWorkflowsToRun').append('<option value="' +
                        workflows[i].WorkflowID + '">' + workflows[i].WorkflowName + '</option>');
                });

                PopModal($('#divLoadRunWorkflowModal'), 'show', 'auto', true);
                GetWorkflowItems($('#selWorkflowsToRun').val());
            }
            else {
                ShowMessage(result.Error, 'show');
            }
        }
    });
}

function sortWFItems(wfi, prop, asc) {
    wfi = wfi.sort(function (a, b) {
        if (asc) {
            return (a[prop] > b[prop]) ? 1 : ((a[prop] < b[prop]) ? -1 : 0);
        } else {
            return (b[prop] > a[prop]) ? 1 : ((b[prop] < a[prop]) ? -1 : 0);
        }
    });

    return wfi;
}

function GetWorkflowItems(wfid) {
    $('#divWorkflowItems').html('');

    var wfi;

    for (var i = 0; i < workflows.length; i++) {
        if (workflows[i].WorkflowID === parseInt(wfid)) {
            wfi = workflows[i].WorkflowItems;
        }
    }

    if (wfi !== undefined && wfi.length > 0) {
        wfi = sortWFItems(wfi, 'RunOrder', true);
    }

    if (wfi === undefined || wfi.length === 0) {
        $('#btnRunWorkflow').attr('disabled', 'disabled');

        $('#divWorkflowItems').append('<label style="color: red;">There are no workflow items assigned to that workflow.</label>');
    }
    else {
        $('#btnRunWorkflow').removeAttr('disabled');

        $.each(wfi, function (item, w) {
            if (w.Disabled === false && w.LibraryItem.Disabled === false) {
                var wfItemID = w.WFItemID;
                var runOrder = w.RunOrder;
                var ItemID = w.ItemID;

                var libItems = w.LibraryItem;
                var libItemID = libItems.ItemID;
                var libName = libItems.ItemName;
                var libDesc = libItems.ItemDesc;
                var htmlOptions = libItems.HtmlOptions;

                $('#divWorkflowItems').append('<div title="' + libDesc + '"><input type="checkbox" id="chkItem_' + libItemID + '" class="WorkflowItemsToRun" checked="checked" value="' + wfItemID + '" />' +
                    '<label>' + libName + '</label><div class="divHtmlOptions" data-val-id="' + libItemID + '" id="divOptions_' + libItemID + '" style="text-align: left; padding-left: 15px; margin-top: 5px;"></div></div>');

                $('#chkItem_' + libItemID).click(function () {
                    if ($(this).is(':checked')) {
                        $('#divOptions_' + libItemID).css('display', 'block');
                    }
                    else {
                        $('#divOptions_' + libItemID).css('display', 'none');
                    }
                });

                if (htmlOptions !== null || htmlOptions !== "") {
                    $('#divOptions_' + libItemID).css('display', 'block');
                    $('#divOptions_' + libItemID).html(htmlOptions);
                }
                else {
                    $('#divOptions_' + libItemID).css('display', 'none');
                    $('#divOptions_' + libItemID).html('');
                }
            }
        });
    }
}

function RunWorkflow() {
    var wfid = $('#selWorkflowsToRun').val();
    var itemIDs = new Array();
    var htmlOptions = new Array();
    var runDate = $('#txtRunDate').val();

    $('.WorkflowItemsToRun:checkbox:checked').map(function () {
        itemIDs.push($(this).val());
    });

    if (itemIDs.length === 0) {
        ShowMessage('You must select at least one workflow item.', 'show');
        return false;
    }

    $('.divHtmlOptions').each(function () {
        if ($(this).is(':visible')) {

            $(this).find(':input').each(function () {
                if ($(this).val().indexOf("&") > -1) {
                    ShowMessage('You cannot include "&" in the data, that is a reserved character.', 'show');
                    return false;
                }

                if ($(this).val().indexOf("=") > -1) {
                    ShowMessage('You cannot include "=" in the data, that is a reserved character.', 'show');
                    return false;
                }
            });

            var id = $(this).attr("data-val-id");
            var data = $(this).find(':input').serialize();

            if (data !== "") {
                htmlOptions.push(id + ':' + data);
            }
        }
    });

    var empID = $('#hidRunWorkflowEmpID').val();

    $.ajax({
        url: '/Dashboard/RunWorkflow',
        data: { EmpID: empID, WorkflowID: wfid, ItemIDs: itemIDs, HtmlOptions: htmlOptions, RunDate: runDate },
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                PopModal($('#divLoadRunWorkflowModal'), 'hide');
                var tab = $('#GetRuns');
                SetTab(tab);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function AddDays() {
    var numDays = $('#txtAddDays').val();
    var dateToday = new Date(new Date().getTime() + (numDays * 24 * 60 * 60 * 1000));

    var dd = dateToday.getDate();
    var mm = dateToday.getMonth() + 1;
    var y = dateToday.getFullYear();

    var finalDate = mm + '/' + dd + '/' + y;

    $('#txtRunDate').val(finalDate);
}