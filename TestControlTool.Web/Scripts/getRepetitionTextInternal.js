function GetInterval(value, period) {
    var maximal = 0;
    var result = new Array();

    switch (period) {
        case "0":
            maximal = 31;
            break;

        case "1":
            maximal = 12;
            break;

        case "2":
            maximal = 7;
            break;

        case "3":
            maximal = 3000;
            break;
    }

    if (value == "*") return GenerateInterval(1, maximal);
    if (value == "!") return GenerateInterval(1, -1);

    var intervals = value.split(',');

    if (intervals.length > 1) {
        for (var i = 0; i < intervals.length; i++) {
            var intervalResults = GetInterval(intervals[i], period);

            for (var j = 0; j < intervalResults.length; j++) {
                result.push(intervalResults[j]);
            }
        }

        return result;
    }

    var duration = value.split('-');

    if (duration.length == 2) {
        var left = duration[0];
        var right = duration[1];

        if (left < right) {
            return GenerateInterval(left, right);
        }
    }

    result.push(value);

    return result;
}

function GenerateInterval(start, end) {
    var result = new Array();

    for (var i = start; i <= end; i++) {
        result.push(i);
    }

    return result;
}

function getRepetitionTextInternal(time, frequency) {
    var days = ["Sunday", "Monday", "Tuesday", "Wednsday", "Thursday", "Friday", "Saturday"];

    if (frequency == "*") return "Everyday at " + time;
    if (frequency == "!") return "Never";

    var intervals = GetInterval(frequency, "2");

    var repetition = "";

    for (var i = 0; i < intervals.length; i++) {
        repetition += days[intervals[i]] + ", ";
    }

    repetition = repetition.replace(/, $/, "");

    repetition += " at " + time;

    return repetition;
}