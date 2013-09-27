var isEditDeployInstallModal = false;
var isEditTestSuiteModal = false;
var suiteType = "UISuiteTrunk";
var testSuiteToEdit;

function editDeployTask(name) {
    isEditDeployInstallModal = true;

    $('#DeployInstallJobLabel').text('Edit DeployInstall Job');
    $('#DeployInstallJobName').val(name);
    $('#DeployInstallJobType').val($('#' + name).attr('deployType'));
    $('#DeployInstallJobVersion').val($('#' + name).attr('version'));
    $('#DeployInstallJobBuild').val($('#' + name).attr('build'));
    $('#EditDeployInstallJobName').val(name);
    $('#SaveDeployInstallJobButton').val('Apply');

    var machines = $('#' + name).attr('machines').split(';');

    for (var i = 0; i < machines.length; i++) {
        $('#' + machines[i]).attr('checked', true);
    }

    $('#DeployInstallJobModal').modal('show');
}

function editTestSuiteTask(obj) {
    var parent = $(obj).parent();
    
    isEditTestSuiteModal = true;
    testSuiteToEdit = parent;
    
    var id = parent.attr('id');
    suiteType = parent.attr('type');
    
    $('#TestSuiteLabel').text('Edit ' + GetNameFromSuiteType(suiteType));
    $('#TestSuiteName').val(id);
    $('#EditTestSuiteName').val(id);
    $('#TestSuiteMachine').val(parent.attr('machine'));
    $('#SaveTestSuiteButton').val('Apply');

    $('#testsList').empty();

    var jsonObject = JSON.parse(parent.attr('json'));

    for (var i in jsonObject) {
        var arrayItem = jsonObject[i];
        
        if (arrayItem instanceof Function) {
            continue;
        }
        
        var object = arrayItem.object;
        var type = arrayItem.type;
        
        var name = type.split('.').last();

        var li = "<li testtype='" + type + "' json='" + JSON.stringify(object) + "'><a href='#' onclick=\"editTestModal(this); return false;\">" + name + "</a><button type='button' class='close' aria-hidden='true' onclick='removeItem(this)'>×</button></li>";
        
        $('#testsList').append(li);
    }

    $('#TestSuiteModal').modal('show');
}

function showNewTestSuite(type) {
    suiteType = type;

    $('#TestSuiteLabel').text('Add new ' + GetNameFromSuiteType(type));

    $('#TestSuiteModal').modal('show');
}

var alreadyChildren = new Array();

function showUploadTestSuiteModal(type) {
    alreadyChildren = new Array();

    suiteType = type;

    $('#currentTasks').children('li').each(function () {
        alreadyChildren.push($(this).attr('id'));
    });

    $('#UploadTestSuiteXmlModal').modal('show');
}

function removeItem(obj) {
    $(obj).parent().fadeOut('slow', function () {
        $(this).remove();
    });
}

function saveTasks(id, urlToSave, urlToRedirect) {
    $('#taskForm').validate().element($('#Task_Name'));
    var val = $('#taskForm').validate();
    val.showErrors();
    if (!val.valid()) {
        showErrorAllert("Please, verify your data");
        
        return false;
    }

    var taskName = $('#Task_Name').val();
    var startTime = $('#Task_StartTime').val();
    var endTime = $('#Task_EndTime').val();
    var frequency = $('#Task_Frequency').val();
    var isEnabled = $('#Task_IsEnabled').is(':checked');
    
    if (new Date(startTime).getHours() >= 20) {
        showErrorAllert("Sorry, but you aren\'t allowed to create task, which starts between 8 PM and 12 PM. It is a maintenance time");

        return false;
    }
    
    var model = { id: id, name: taskName, startTime: startTime, endTime: endTime, frequency: frequency, isEnabled: isEnabled };
    var tasks = getTasks();

    var data = { model: model, tasks: tasks };

    $('#SaveButton').html('Save <img src="/Content/images/select2-spinner.gif"/>');

    $.ajax({
        url: urlToSave,
        data: { jsonModel: JSON.stringify(data) },
        //traditional: true,
        type: "POST",
        dataType: "json",
        success: function (data, textStatus) {
            if (data.toString().toLowerCase() == 'true') {
                window.location.href = urlToRedirect;
            } else {
                showErrorAllert(data);
            }
        },
        complete: function (jqXhr, textStatus) {
            $('#SaveButton').html('Save');
        }
    });

    return false;
}

function getTasks() {
    var tasks = [];

    $('#currentTasks').children('li').each(function () {
        if ($(this).is(":visible")) {
            var attributes = {};

            var listAttributes = $(this).listAttributes();

            for (var i = 0; i < listAttributes.length; i++) {
                var n = listAttributes[i];

                var a = $(this).attr(n);
                
                attributes[n] = a;
            }

            tasks.push(attributes);
        }
    });

    return tasks;
}

function showErrorAllert(message) {
    $('#alertArea').append("<div class='alert alert-block alert-error fade in' id='errorAlert'>" +
        "<button type='button' class='close' data-dismiss='alert'>×</button>" +
        "<h4 class='alert-heading'>You've got some errors!</h4>" +
        "<p>" + message + "</p>");

    $('#errorAlert').bind('closed', function () {
        if ($('#alertArea').children().length == 0) {
            $('#alertArea').fadeOut('slow');
        }
    });

    $('#alertArea').fadeIn('slow');
}

$('#Task_StartTime').popover({
    title: "Start time help",
    animation: true,
    placement: 'bottom',
    content: 'Start time is represented in Cron time format. You can read about it <a href="http://www.nncron.ru/help/EN/working/cron-format.htm" target="_blank">here</a>',
    html: true,
    trigger: 'manual'
});

$('#Task_StartTime').focus(function () {
    $('#Task_StartTime').popover('show');
});

$('#Task_StartTime').blur(function () {
    $('#Task_StartTime').popover('hide');
});

function validate() {

    var isValid =  $('#taskForm').validate().element($('#Task_Name'));

    return isValid;
}

function parseFrequncy(frequncy) {
    var timeParts = frequncy.split(' ');

    var daysIntervals = GetInterval(timeParts[2], "2");

    var daysCheckBoxes = $('#allDaysList').find('input[type=checkbox]');

    for (var i = 0; i < daysIntervals.length; i++) {
        $(daysCheckBoxes[daysIntervals[i] == 7 ? 0 : daysIntervals[i]]).attr('checked', true);
    }

    if (daysIntervals.length == 7) $('#AllDaysCheckBox').attr('checked', true);
}

function getRepetitonText() {
    var frequency = $('#Task_Frequency').val().split(' ')[2];
    var time = (new Date($('#Task_StartTime').val())).toLocaleTimeString();

    return getRepetitionTextInternal(time, frequency);
}

function showCalendar() {
    $('#StartDate').data('datetimepicker').setLocalDate(new Date($('#Task_StartTime').val()));
    $('#StartTime').data('datetimepicker').setLocalDate(new Date($('#Task_StartTime').val()));
    $('#EndDate').data('datetimepicker').setLocalDate(new Date($('#Task_EndTime').val()));

    parseFrequncy($('#Task_Frequency').val());

    $('#CalendarModal').modal('show');
}

function s4() {
    return Math.floor((1 + Math.random()) * 0x10000)
               .toString(16)
               .substring(1);
};

function guid() {
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
           s4() + '-' + s4() + s4() + s4();
}

function isString(o) {
    return typeof o == "string" || (typeof o == "object" && o.constructor === String);
}

function GetNameFromSuiteType(suiteType) {
    if (suiteType == "BackendSuiteTrunk") {
        return "Backend Test Suite for Trunk";
    }
    
    if (suiteType == "BackendSuiteRelease") {
        return "Backend Test Suite for Release";
    }
    
    if (suiteType == "UISuiteTrunk") {
        return "UI Test Suite for Trunk";
    }
    
    if (suiteType == "UISuiteRelease") {
        return "UI Test Suite for Release";
    }

    return "";
}

function getJsonForForm(formId) {
    var jsonObject = '{';
    
    $('#' + formId).children('div').not('.wellHelper').find('input[type!=hidden][type!=checkbox],select').each(function () {
        jsonObject += ' "' + trimId($(this).attr('id')) + '" : "' + encodeValue($(this).val()) + '",';
    });

    $('#' + formId).children('div').not('.wellHelper').find('input[type=checkbox]').each(function () {
        jsonObject += ' "' + trimId($(this).attr('id')) + '" : "' + $(this).is(':checked') + '",';
    });

    $('#' + formId).children('.wellHelper').children('.collapse').each(function () {
        jsonObject += ' "' + trimId($(this).attr('id')) + '" : ' + getJsonForForm($(this).attr('id')) + ',';
    });
    
    $('#' + formId).children('.wellList').find('ul').each(function () {
        jsonObject += ' "' + trimId($(this).attr('id')) + '" : [ ';

        $(this).children('li').each(function() {
            jsonObject += $(this).attr('json') + ',';
        });

        jsonObject = jsonObject.replace(/,$/, "") + ' ],';
    });

    jsonObject = jsonObject.replace(/,$/, "") + '}';

    return jsonObject;
}

function fillFormWithObject(formId, object) {
    for (var id in object) {
        var item = findChildElement(formId, id);
        var param = object[id];
        
        if (isString(param)) {
            if (item.is(':checkbox')) {
                item.attr('checked', param.toLowerCase() == 'true');
            } else {
                item.val(decodeValue(param));
            }

            item.trigger("change");
        }
        else if (param instanceof Array) {
            for (var arrayId in param) {
                var liItem = param[arrayId];
                
                if (!(liItem instanceof Function)) {
                    item.append("<li json='" + JSON.stringify(liItem) + "'><a href='#' onclick='editListItem(this); return false;'>" + findFirstNonObjectParam(liItem) + "</a><button type='button' class='close' aria-hidden='true' onclick='removeItem(this)'>×</button></li>");
                }
            }
        }
        else {
            fillFormWithObject(item.attr('id'), param);
        }
    }
}

function findFirstNonObjectParam(object) {
    if (isString(object)) {
        return object;
    } else {
        for (var param in object) {
            if (isString(object[param])) {
                return object[param];
            }
        }
    }

    return undefined;
}

function trimId(id) {
    return id.split('-').last();
}

function findChildElement(formId, elementId) {
    var result = undefined;
    
    result = $('#' + formId).find(':not("button")[id$="' + elementId + '"]').each(function () {
        var end = $(this).attr('id').split('-').last();
        
        if (end == elementId) return $(this);
    });

    return result;
}

function encodeValue(value) {
    return value.replace("'", "&apos;").replace('"', '&quot;').replace('\\', '\\\\');
}

function decodeValue(value) {
    return value.replace("&apos;", "'").replace('&quot;', '"').replace('\\\\', '\\');
}

function deniedStarts(start) {
    if (start == 'itemsProperties' || start == 'ListItemsModal' || start == 'AddButton') return true;

    return false;
}