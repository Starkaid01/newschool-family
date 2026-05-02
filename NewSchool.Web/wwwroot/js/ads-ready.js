window.newSchoolAds = window.newSchoolAds || {};

window.newSchoolAds.push = function () {
    try {
        if (window.adsbygoogle) {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
        }
    } catch (error) {
        if (window.console && typeof window.console.debug === "function") {
            window.console.debug("NewSchool ads push skipped.", error);
        }
    }
};

window.newSchoolAds.init = function () {
    return true;
};

document.addEventListener("DOMContentLoaded", function () {
    if (window.newSchoolAds && typeof window.newSchoolAds.init === "function") {
        window.newSchoolAds.init();
    }
});
