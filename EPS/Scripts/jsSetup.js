function OpenManageWFItemsModal() {
    $("#divManageWFItemsContent").load('Dashboard/ManageWFItems', function (response) {
        if (response.indexOf("<script type='text/javascript'>ShowMessage") !== 0) {
            PopModal($('#divManageWFItemsModal'), 'show', 'auto');
            $.validator.unobtrusive.parse($('#divManageWFItemsModal'));
        }
    });    
}

function OpenNewWorkflowModal() {
    $('#divNewWorkflowContent').load('Dashboard/AddWorkflow', function (response) {
        if (response.indexOf("<script type='text/javascript'>ShowMessage") !== 0) {
            $.validator.unobtrusive.parse($('#divNewWorkflowModal'));
            PopModal($('#divNewWorkflowModal'), 'show', 'auto');
        }
    });        
}

function OpenChangeLibraryItemFileModal(itemID) {
    $('#divReplaceLibraryItemFileContent').load('Dashboard/ChangeLibraryItemFile?ItemID=' + itemID, function (response) {
        if (response.indexOf("<script type='text/javascript'>ShowMessage") !== 0) {
            $.validator.unobtrusive.parse($('#divReplaceLibraryItemFileModal'));
            PopModal($('#divReplaceLibraryItemFileModal'), 'show', 'auto');
        }
    });
}

function WorkflowAdded() {
    PopModal($('#divNewWorkflowModal'), 'hide', 'auto');

    var tab = $('#GetSetup');
    SetTab(tab);
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

function AddWorflowItemToWF(wfid) {
    var itemID = $('#ddlAddWorkflowItemToWF_' + wfid).val();

    $.ajax({
        url: '/Dashboard/AddWorflowItemToWF',
        type: 'POST',
        data: { WorkflowID: wfid, ItemID: itemID },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                var tab = $('#GetSetup');
                SetTab(tab);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function ChangeRunOrder(wfItemID, direction, focusItem) {
    $.ajax({
        url: '/Dashboard/ChangeRunOrder',
        type: 'POST',
        data: { WFItemID: wfItemID, Direction: direction },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                var tab = $('#GetSetup');
                SetTab(tab);

                focusItem.focus();
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function RemoveItemFromWF(wfItemID) {
    var conf = confirm("Are you sure you want to remove that item from the workflow? Any audit log entries will be removed as well.");
    if (conf === false) {
        return;
    }

    $.ajax({
        url: '/Dashboard/RemoveItemFromWF',
        type: 'POST',
        data: { WFItemID: wfItemID },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                $('#trWFItem_' + wfItemID).remove();
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function EditWorkflow(id) {
    $('.NoEdit_' + id).each(function () { $(this).css('display', 'none'); });
    $('.Edit_' + id).each(function () { $(this).css('display', 'inline-block'); });
}

function SaveWorkflow(id) {
    var name = $('#txtWFName_' + id).val();
    var desc = $('#txtWFDesc_' + id).val();

    if (name === '') {
        ShowMessage('You must enter a workflow name.', 'show');
        return false;
    }

    if (desc === '') {
        ShowMessage('You must enter a workflow description.', 'show');
        return false;
    }

    $.ajax({
        url: '/Dashboard/UpdateWorkflow',
        type: 'POST',
        data: { WorkflowID: id, WorkflowName: encodeURIComponent(name), WorkflowDesc: encodeURIComponent(desc) },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                $('#lblWFName_' + id).text(name);
                $('#lblWFDesc_' + id).text(desc);

                $('.NoEdit_' + id).each(function () { $(this).css('display', 'inline-block'); });
                $('.Edit_' + id).each(function () { $(this).css('display', 'none'); });
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function CancelEditWorkflow(id) {
    $('.NoEdit_' + id).each(function () { $(this).css('display', 'inline-block'); });
    $('.Edit_' + id).each(function () { $(this).css('display', 'none'); });
}