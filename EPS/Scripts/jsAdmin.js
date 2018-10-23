$(document).ready(function () {
    $("#tabs_admin li").click(function () {
        TabPressed_Admin($(this));
    });

    SetTab_Admin($('#Users'));
});

function TabPressed_Admin(tab) {
    $(document).attr("title", "EPS - " + tab.text());
    SetTab_Admin(tab);
}

function SetTab_Admin(tab) {
    var id = tab.attr('id');

    $("#tabs_admin li").removeClass('active');
    tab.addClass("active");
    $(".tab_content_admin").hide();
    var selected_tab = tab.find("a").attr("href");

    $('#divMainAdmin').fadeIn("300", function () {
        LoadAdmin(selected_tab.replace('#', ''));
    });
}

function LoadAdmin(item) {
    $.ajaxSetup({
        cache: false
    });

    $("#divMainAdmin").load('Dashboard/' + item);
}

function DeleteUser(userID) {
    var conf = confirm("Are you sure you want to delete that user?");
    if (conf === false) {
        return;
    }

    $.ajax({
        url: '/Dashboard/DeleteUser',
        type: 'POST',
        data: { UserID: userID },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                var tab = $('#GetAdmin');
                SetTab(tab);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function LoadSyncAD() {
    PopModal($('#divSyncADAdminModal'), 'show', 'auto', true, false, 'FirstName');
}

function AddUser(ADGUID, Email, FirstName, LastName, Username) {
    $.ajax({
        url: '/Dashboard/AddUser?ADGUID=' + ADGUID + '&Email=' + Email + '&FirstName=' + FirstName + '&LastName=' + LastName + '&Username=' + Username,
        type: 'POST',
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                $('#tblAdmins').append('<tr><td>' + Username + '</td><td>' + FirstName + '</td><td>' + LastName + '</td><td>' + Email +
                    '</td><td><input type="button" class="RedButton" value="Remove User" onclick="DeleteUser(' + data.UserID + ');" /></td></tr>');

                $('#lblAddEmp_' + Username).css('display', 'block');
                $('#btnAddEmp_' + Username).css('display', 'none');

                ShowMessage('User Added', 'show', false);
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function SetParamEdit(edit, id) {
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

function ParamAdded() {
    $('#ParamName').val('');
    $('#ParamDesc').val('');
    $('#ParamValue').val('');

    SetTab_Admin($('#Parameters'));
}

function SaveParam(id) {
    var name = $('#txtParamName_' + id).val();
    var desc = $('#txtParamDesc_' + id).val();
    var value = $('#txtParamValue_' + id).val();

    if (name === "") { ShowMessage('A name is required.', 'show'); return false; }
    if (desc === "") { ShowMessage('A description is required.', 'show'); return false; }
    if (value === "") { ShowMessage('A value is required.', 'show'); return false; }

    $.ajax({
        url: '/Dashboard/EditParam',
        type: 'POST',
        data: { ParamID: id, ParamName: encodeURIComponent(name), ParamDesc: encodeURIComponent(desc), ParamValue: encodeURIComponent(value) },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                SetTab_Admin($('#Parameters'));
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function DeleteParam(id) {
    var conf = confirm("Are you sure you want to delete that parameter? It may be used by a workflow item.");
    if (conf === false) {
        return;
    }

    $.ajax({
        url: '/Dashboard/DeleteParam',
        type: 'POST',
        data: { ParamID: id },
        datatype: 'json',
        cache: false,
        success: function (data) {
            if (data.Error === "") {
                SetTab_Admin($('#Parameters'));
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        }
    });
}

function ParamUpdated() {
    SetTab_Admin($('#Parameters'));
}

function ParamDeleted() {
    SetTab_Admin($('#Parameters'));
}