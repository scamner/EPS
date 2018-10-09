$(document).ready(function () {
    $("#divLoading").dialog({
        buttons: {
            "open": {
                id: "btnOpenLoading",
                style: "display: none;",
                click: function () {
                    $(this).dialog().dialog('open');
                }
            },
            "cancel": {
                id: "btnCloseLoading",
                style: "display: none;",
                click: function () {
                    $(this).dialog().dialog('close');
                }
            }
        },
        modal: true,
        autoOpen: false,
        width: "400px",
        resizable: false,
        dialogClass: "EPS_Modal"
    });

    startLoginCheck();
})

var checkLoginIntDefault = 45000;
var checkLoginInt = checkLoginIntDefault;
var timerLoginCheck = null;

function startLoginCheck() {
    var page = window.location.href;

    if (timerLoginCheck == null && page.indexOf("/User/") == -1) {
        timerLoginCheck = window.setTimeout(function () {
            checkLoginTimeout();
        }, checkLoginInt);
    }
}

function checkLoginTimeout() {
    var page = window.location.href;

    clearTimeout(timerLoginCheck, checkLoginTimeout);

    $.ajax(eps_Root + "CheckSession/CheckLoginTimeout").done(function (data) {
        if (parseInt(data) <= 180) {
            $('#txtTimeoutText').html('You have ' + Math.ceil(parseInt(data) / parseInt(60)) + ' minute(s) left before you will be logged out.<br />Click the button below to continue working.');

            checkLoginInt = 15000;
            startLogout(parseInt(data));
        } else {
            checkLoginInt = checkLoginIntDefault;
            timerLoginCheck = null;
            startLoginCheck();
        }

        timerLoginCheck = null;
    });
}

function startLogout(seconds) {
    clearTimeout(timerLoginCheck, checkLoginTimeout);

    if (seconds <= 15) {
        window.open(eps_Root + "User/Logout", "_self");
    }
    else {
        if (!$("#divLogonTimeout").is(":visible")) {
            PopModal($("#divLogonTimeout"), 'show', 'auto', true, 'btnLogonTimeout');
        }

        timerLoginCheck = null;
        startLoginCheck();
    }
}

function resetTimeout() {
    try {
        var page = window.location.href;

        $.ajax(eps_Root + "CheckSession/ResetTimeout").done(function (data) {
            if (data.Error == "") {
                checkLoginInt = checkLoginIntDefault;

                if (!$("#divLogonTimeout").is(":visible")) {
                    PopModal($("#divLogonTimeout"), 'show', 'auto', true, 'btnLogonTimeout');
                }

                clearTimeout(timerLoginCheck, checkLoginTimeout);
                timerLoginCheck = null;
                startLoginCheck();
            }
            else {
                ShowMessage(data.Error, 'show');
            }
        });
    }
    catch (err) {
        alert(err.message);
    }
}

function ShowMessage(message, showHide, hideCloseButton) {
    var txtMessageText = $("#txtMessageText");
    txtMessageText.html(message);

    if (showHide == 'show') {
        PopModal($("#divMessage"), 'show', 'auto', hideCloseButton, 'txtMessageText');
    }
    else {
        PopModal($("#divMessage"), 'hide', 0, hideCloseButton);
    }
};

$(document).ajaxSend(function (evt, request, settings) {
    var showLoading = true;
    var url = settings.url;

    if (url.indexOf("CheckSession/") > -1) {
        showLoading = false;
    }

    if (url.indexOf("/GetRuns") > -1) {
        showLoading = false;
    }

    if (showLoading == true) {
        var btnOpenLoading = $("#btnOpenLoading");
        btnOpenLoading.click();
    }
}).ajaxStop(function () {
    var btnCloseLoading = $("#btnCloseLoading");
    btnCloseLoading.click();

    var ctlToFocus = $('#hidFocusControl').val();
    if (ctlToFocus != '') {
        $('#' + ctlToFocus).focus();
    }
});

function PopModal(control, showHide, width, top, hideCloseButton, controlToFocus) {
    var className = "EPS_Modal";

    if (top == true) {
        className = "EPS_Modal EPS_Modal_Top";
    }

    if (showHide == 'show') {
        control.dialog({
            modal: true,
            resizable: false,
            autoOpen: true,
            hide: 'fold',
            show: 'blind',
            width: width,
            dialogClass: className,
        });

        if (width == 'auto') {
            control.dialog("option", "width", 'auto');
        }

        if (hideCloseButton === true) {
            $("#btnCloseMessage").css("display", "none");
        }
        else {
            $("#btnCloseMessage").css("display", "block");
        }

        if (controlToFocus != null) {
            $('#' + controlToFocus).focus();
        }
    }
    else {
        control.dialog("destroy");
    }
};



