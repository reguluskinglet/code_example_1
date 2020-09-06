using System.ComponentModel;
#pragma warning disable 1591

namespace demo.DemoApi.Domain.StatusCodes
{
    /// <summary>
    /// Коды ошибок
    /// </summary>
    public enum ErrorCodes: uint
    {
        [Description("Unknown")]
        Unknown = 0,

        [Description("Parameter Validation Error")]
        ValidationError = 1,

        [Description("Applicant already in call")]
        CallBackToApplicantAlreadyInCall = 2,

        [Description("User not found")]
        UserNotFound = 3,

        [Description("Line not found")]
        LineNotFound = 4,

        [Description("Case folder page not found")]
        CaseFolderPageNotFound = 5,

        [Description("Case folder not found")]
        CaseFolderNotFound = 6,

        [Description("Case not found")]
        CaseNotFound = 7,

        [Description("Key for cache not found")]
        KeyForCacheNotFound = 8,

        [Description("IndexTree not found")]
        IndexTreeNotFound = 9,

        [Description("Inbox type not defined")]
        InboxTypeNotDefined = 10,

        [Description("Unable to get location data")]
        UnableToGetLocationData = 11,

        [Description("Unable to close case card")]
        UnableToCloseCaseCard = 12,

        [Description("Unable to set case status")]
        UnableToSetCaseStatus = 13,

        [Description("Unable to add case card to case folder")]
        UnableToAddCaseCardToCaseFolder = 14,

        [Description("Unable to fill sms data")]
        UnableToFillSmsData = 15,

        [Description("Unable to add data to cache")]
        UnableToAddDataToCache = 16,

        [Description("Unable to remove data to cache")]
        UnableToRemoveDataToCache = 17,

        [Description("Unable to get applicant")]
        UnableToGetApplicant = 18,

        [Description("Unable to get journal calls")]
        UnableToGetJournalCalls = 19,

        [Description("Unable to exchange user roles")]
        UnableToExchangeUserRoles = 20,

        [Description("Unable to set isolation status")]
        UnableToSetIsolationStatus = 21,

        [Description("Unable to add call to line")]
        UnableToAddCallToLine = 22,

        [Description("Unable to add user group call to line")]
        UnableToAddUserGroupCallToLine = 23,

        [Description("Unable to microphone change state")]
        UnableToMicrophoneChangeState = 24,

        [Description("Unable to set hold status")]
        UnableToSetHoldStatus = 25,

        [Description("Unable to call back to applicant")]
        UnableToCallBackToApplicant = 26,

        [Description("Unable to call to number")]
        UnableToCallToNumber = 27,

        [Description("Unable to accept call")]
        UnableToAcceptCall = 28,

        [Description("Unable to get user lines")]
        UnableToGetUserLines = 29,

        [Description("Unable to get contacts page")]
        UnableToGetContactsPage = 30,

        [Description("Unable to get location Espg3857")]
        UnableToGetLocationEspg3857 = 31,

        [Description("Unable to add applicant location marker")]
        UnableToAddApplicantLocationMarker = 32,

        [Description("Unable to add incident marker to map")]
        UnableToAddIncidentMarkerToMap = 33,

        [Description("Unable to get index")]
        UnableToGetIndex = 34,

        [Description("User already authorized")]
        UserAlreadyAuthorized = 35,

        [Description("SMS already taken")]
        // ReSharper disable once InconsistentNaming
        SMSAlreadyTaken = 36,

        [Description("Inbox not found")]
        InboxNotFound = 37,

        [Description("Language not found")]
        LanguageNotFound = 38,

        [Description("Languages not found")]
        LanguagesNotFound = 39,

        [Description("Unable to get Inbox")]
        UnableToGetInbox = 40,

        [Description("Unable to get Token")]
        UnableToGetToken = 41,

        [Description("CaseType not found")]
        CaseTypeNotFound = 42,
        
        [Description("Call was accepted by another user")]
        CallAlreadyAccepted = 43,

        [Description("Unable to get lines")]
        UnableToGetLines = 44,

        [Description("Unable to get calls in line")]
        UnableToGetCallsInLine = 45,

        [Description("Unable to get audio file stream")]
        UnableToGetAudioFileStream = 46,

        [Description("Unable to get audio records")]
        UnableToGetAudioRecords = 47,

        [Description("Unable to get user roles")]
        UnableToGetRoles = 48,

        [Description("Unable to get work choices")]
        UnableToGetWorkChoices = 49,

        [Description("Unable to get connection receivers")]
        UnableToGetReceivers = 50,
    }
}
