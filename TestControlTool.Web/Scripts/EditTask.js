var isEditDeployInstallModal = false;
var isEditTestSuiteModal = false;
var isTrunk = true;

function editDeployTask(name) {
    isEditDeployInstallModal = true;

    $('#DeployInstallJobLabel').text('Edit DeployInstall Job');
    $('#DeployInstallJobName').val(name);
    $('#DeployInstallJobType').val($('#' + name).attr('deployType'));
    $('#DeployInstallJobVersion').val($('#' + name).attr('version'));
    $('#EditDeployInstallJobName').val(name);
    $('#SaveDeployInstallJobButton').val('Apply');

    var machines = $('#' + name).attr('machines').split(';');

    for (var i = 0; i < machines.length; i++) {
        $('#' + machines[i]).attr('checked', true);
    }

    $('#DeployInstallJobModal').modal('show');
}

function editTestSuiteTask(obj) {
    isEditTestSuiteModal = true;
    
    var parent = $(obj).parent();
    
    var name = parent.attr('id');
    isTrunk = parent.attr('type') == 'TestSuiteTrunk';
    
    $('#TestSuiteLabel').text('Edit Test Suite ' + (isTrunk ? 'Trunk' : 'Release'));
    $('#TestSuiteName').val(name);
    $('#EditTestSuiteName').val(name);
    $('#TestSuiteMachine').val(parent.attr('machine'));
    $('#SaveTestSuiteButton').val('Apply');

    $('#' + name).find('ol').first().children('li').each(function (index) {
        $('#testsList').append($(this).clone());
    });

    $('#TestSuiteModal').modal('show');
}

function showNewTestSuite(trunk) {
    isTrunk = trunk;

    $('#TestSuiteLabel').text('Add new Test Suite ' + (trunk ? 'Trunk' : 'Release'));

    $('#TestSuiteModal').modal('show');
}

var alreadyChildren = new Array();

function showUploadTestSuiteModal(trunk) {
    alreadyChildren = new Array();

    isTrunk = trunk;

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
        showErrorAllert();
    }

    if (!val.valid()) return false;

    var tasks = getTasks();
    var taskName = $('#Task_Name').val();
    var startTime = $('#Task_StartTime').val();
    var endTime = $('#Task_EndTime').val();
    var frequency = $('#Task_Frequency').val();
    var isEnabled = $('#Task_IsEnabled').is(':checked');

    $.ajax({
        url: urlToSave,
        data: { jsonModel: JSON.stringify({ tasks: JSON.stringify(tasks["tasks"]), tests: JSON.stringify(tasks["tests"]), parameters: JSON.stringify(tasks["parameters"]), model: JSON.stringify({ id: id, name: taskName, startTime: startTime, endTime: endTime, frequency: frequency, isEnabled : isEnabled }) }) },
        //traditional: true,
        type: "POST",
        dataType: "json",
        success: function (data, textStatus) {
            if (JSON.parse(data) == true) {
                window.location.href = urlToRedirect;
            } else {
                showErrorAllert();
            }
        }
    });

    return false;
}

function getTasks() {
    var global = {};
    var tasks = {};

    $('#currentTasks').children('li').each(function () {
        if ($(this).is(":visible")) {
            var attributes = {};

            var listAttributes = $(this).listAttributes();

            for (var i = 0; i < listAttributes.length; i++) {
                var n = listAttributes[i];
                var a = $(this).attr(n).replace(',', ';');

                attributes[n] = a;
            }

            tasks[$(this).attr('id')] = attributes;
        }
    });

    var listTests = {};
    var listComplexParameters = { };

    $('#currentTasks').find('[type^=TestSuite]').each(function () {
        var tests = $(this).find('ol').first();
        
        var listOl = new Array();

        tests.children('li').each(function () {
            var listLi = {};

            var id = s4();
            
            var listAttributes = $(this).listAttributes();

            for (var i = 0; i < listAttributes.length; i++) {
                var n = listAttributes[i];
                var a = $(this).attr(n).toString().replace(',', ';');

                listLi[n] = a;
            }

            listLi["id"] = id;

            var childListComplexParameters = { };

            $(this).find('ul').each(function () {
                var childListOl = new Array();
                $(this).find('li').each(function () {
                    var childListLi = {};

                    var childListAttributes = $(this).listAttributes();

                    for (var i = 0; i < childListAttributes.length; i++) {
                        var n = childListAttributes[i];
                        var a = $(this).attr(n).toString().replace(',', ';');

                        childListLi[n] = a;
                    }
                    
                    childListOl.push(childListLi);
                });

                childListComplexParameters[$(this).attr('id').replace(/HiddenList$/, "")] = childListOl;
            });

            listComplexParameters[id] = childListComplexParameters;

            listOl.push(listLi);
        });

        listTests[$(this).attr('id')] = listOl;
    });

    global["tasks"] = tasks;
    global["tests"] = listTests;
    global["parameters"] = listComplexParameters;

    return global;
}

function showErrorAllert() {
    $('#alertArea').append("<div class='alert alert-block alert-error fade in' id='errorAlert'>" +
        "<button type='button' class='close' data-dismiss='alert'>×</button>" +
        "<h4 class='alert-heading'>You've got some errors!</h4>" +
        "<p>Please, verify your data</p>");

    $('#errorAlert').bind('closed', function () {
        $('#alertArea').hide();
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