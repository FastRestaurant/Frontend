window.fastRestaurantClock = {
    nowIso: () => new Date().toISOString(),
    timezoneOffsetMinutes: () => new Date().getTimezoneOffset()
};
