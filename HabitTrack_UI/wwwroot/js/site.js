window.getUserTimeZone = () => {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
};
