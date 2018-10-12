function OpenManageWFItemsModal() {
    $("#divManageWFItemsContent").load('Dashboard/ManageWFItems', function (response) {
        if (response.indexOf("<script type='text/javascript'>ShowMessage") !== 0) {
            PopModal($('#divManageWFItemsModal'), 'show', 'auto');
            $.validator.unobtrusive.parse($('#divManageWFItemsModal'));
        }
    });    
}

function OpenNewWorkflowModal() {
    $("#divMain").load('Dashboard/' + item);

    PopModal($('#divNewWorkflowModal'), 'show', 'auto');
}

function SetWorkflowDisabled(workflowID, disabled) {
    var message = disabled === true ? 'disabled.' : 'enabled.';

    $.ajax({
        url: '/Dashboard/SetWorkflowDisabled?WorkflowID=' + workflowID + '&Disabled=' + disabled,
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                ShowMessage('Workflow ' + message, 'show', false);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function SetWorkflowItemDisabled(wfItemID, disabled) {
    var message = disabled === true ? 'disabled.' : 'enabled.';

    $.ajax({
        url: '/Dashboard/SetWorkflowItemDisabled?WFItemID=' + wfItemID + '&Disabled=' + disabled,
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                ShowMessage('Workflow Item ' + message, 'show', false);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function SetManageWFItemEdit(edit, id) {
    if (edit === true) {
        $('.Edit_' + id).each(function () {
            $(this).css('display', 'inline-block');
        });

        $('.NoEdit_' + id).each(function () {
            $(this).css('display', 'none');
        });
    }
    else {
        $('.Edit_' + id).each(function () {
            $(this).css('display', 'none');
        });

        $('.NoEdit_' + id).each(function () {
            $(this).css('display', 'inline-block');
        });
    }
}

function SaveWFItem(id) {
    var name = $('#txtItemName_' + id).val();
    var desc = $('#txtItemDesc_' + id).val();
    var disabled = $('#chkDisabled_' + id).is(':checked');
    var html = $('#txtHtmlOptions_' + id).val();

    if (name === "") { ShowMessage('A name is required.', 'show'); return false; }
    if (desc === "") { ShowMessage('A description is required.', 'show'); return false; }
   
    $.ajax({
        url: '/Dashboard/SaveLibraryItem',
        type: 'POST',
        data: { ItemID: id, ItemName: name, ItemDesc: desc, Disabled: disabled, HtmlOptions: html },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                $("#divManageWFItemsContent").load('Dashboard/ManageWFItems');
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}