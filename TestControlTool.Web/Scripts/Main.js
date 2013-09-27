function IsValidNumberField(input, minimumValue, maximumValue) {
    if ('undefined' == typeof input.previousValue) {
        input.previousValue = input.defaultValue;
    }

    if (input.value < minimumValue || input.value > maximumValue) {
        input.value = input.previousValue;
    } else {
        input.previousValue = input.value;
    }
}

function Disable(obj) {
    var disablingProperties = $(obj).attr('data-disabling-properties').split(';');

    for (var i = 0; i < disablingProperties.length; i++) {
        var disablingProperty = disablingProperties[i].split(',');

        for (var j = 0; j < disablingProperty.length; j++) {
            $("#" + disablingProperty[j]).on("change", function () {
                DisableByAttribute(obj, true);
            });
        }
    }
    
    DisableByAttribute(obj, false);
}

function DisableByAttribute(obj, hide) {
    var disablingProperties = $(obj).attr('data-disabling-properties').split(';');
    var disablingValues = $(obj).attr('data-disabling-values').split(';');

    var disable = false;
    
    for (var i = 0; i < disablingProperties.length; i++) {
        var disablingProperty = disablingProperties[i].split(',');
        var disablingValue = disablingValues[i].split(',');

        var currentValues = getCurrentValues(disablingProperty);
        
        if (compareValues(currentValues, disablingValue)) {
            disable = true;
        }
    }
    
    if (disable) {
        if ($(obj).get(0).tagName != 'DIV') {
            $(obj).prop('disabled', 'disabled');
        } else {
            $(obj).find('input, label, button').each(function () {
                $(this).prop('disabled', 'disabled');
            });

            if (hide) $(obj).collapse('hide');
        }
    } else {
        if ($(obj).get(0).tagName != 'DIV') {
            $(obj).prop('disabled', 'false');
        } else {
            $(obj).find('input, label, button').each(function () {
                $(this).prop('disabled', false);
            });
        }
    }
}

function getCurrentValues(properties) {
    var values = [];

    for (var i = 0; i < properties.length; i++) {
        var value = $("#" + properties[i]).is(':checkbox') ? $("#" + properties[i]).is(':checked').toString() : $("#" + properties[i]).val();

        values.push(value);
    }

    return values;
}

function compareValues(first, second) {
    if (first.length != second.length) return false;

    for (var i = 0; i < first.length; i++) {
        if (first[i].toString().toLowerCase() != second[i].toString().toLowerCase()) return false;
    }

    return true;
}

if (!Array.prototype.last) {
    Array.prototype.last = function() {
        return this[this.length - 1];
    };
}