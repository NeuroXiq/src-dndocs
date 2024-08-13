import originalDayjs, { Dayjs } from "dayjs";

var relativeTime = require('dayjs/plugin/relativeTime')
originalDayjs.extend(relativeTime);


function dayjs(date: string | Date) : any {
    return originalDayjs(date);
}

function parseDate(date: any) : Date | null {
    if (!date) {
        return null;
    }

    if (date instanceof Date) {
        return date;
    }

    if (typeof date === 'string') {
        if (!date.trim()) {
            return null;
        }

        // all dates from API are UTC but without 'Z'
        date = date.endsWith('Z') ? date : date + 'Z';
        
        return new Date(date);
    }

    throw new Error('invalid date value');
}

const dateUI = function (d: any) {
   const date = parseDate(d);

    return !!date ? date.toLocaleDateString() : '';
}

const dateTimeUI = function (date: string | Date | null) {
    date = parseDate(date);

    return !!date ?  date.toLocaleString() : '';
}

function dateTimeAgoUI(date: string | Date | null) {
    date = parseDate(date);

    if (date != null) {
        return dayjs(date).fromNow();
    }

    return '';
}

const nof = <T>(name: keyof T) => name;


export { dateUI, dateTimeUI, dateTimeAgoUI, nof }