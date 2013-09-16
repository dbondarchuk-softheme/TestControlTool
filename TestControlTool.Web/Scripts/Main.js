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

function DisableByAttribute(obj, disablingPropertyName, disablingValue, hide) {
    var currentValue = $("#" + disablingPropertyName).is(':checkbox') ? $("#" + disablingPropertyName).is(':checked').toString() : $("#" + disablingPropertyName).val();

    if (disablingValue.toLowerCase() == currentValue.toLowerCase()) {
        if ($(obj).get(0).tagName != 'DIV') {
            $(obj).prop('disabled', 'disabled');
        } else {
            $(obj).find('input, label').each(function () {
                $(this).prop('disabled', 'disabled');
            });

            if (hide) $(obj).collapse('hide');
        }
    } else {
        if ($(obj).get(0).tagName != 'DIV') {
            $(obj).prop('disabled', 'false');
        } else {
            $(obj).find('input, label').each(function () {
                $(this).prop('disabled', false);
            });
        }
    }
}

if (!Array.prototype.last) {
    Array.prototype.last = function() {
        return this[this.length - 1];
    };
}