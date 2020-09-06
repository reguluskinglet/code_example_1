using System.ComponentModel;
#pragma warning disable 1591

namespace demo.DemoGateway.Client.StatusCodes
{
    /// <summary>
    /// Коды ошибок
    /// </summary>
    public enum ErrorCodes
    {
        [Description("Unknown Code")]
        Unknown = 0,

        [Description("Parameter Validation Error")]
        ValidationError = 1,

        [Description("Incoming call channel not found.")]
        ChannelNotFound = 2,

        [Description("Bridge not found")]
        BridgeNotFound = 3,

        [Description("User not found")]
        UserNotFound = 4,

        [Description("Unable to save Channel")]
        UnableToSaveChannel = 5,

        [Description("Unable to set Isolation status")]
        UnableToSetIsolationStatus = 6,

        [Description("Unable to set Mute status")]
        UnableToSetMuteStatus = 7,

        [Description("Unable to switch main user")]
        UnableToSwitchMainUser = 8,

        [Description("Unexpected Service Response")]
        UnexpectedServiceResponse = 9,

        [Description("Parsing error")]
        ParsingError = 10,

        [Description("Snooping error")]
        SnoopError = 11,

        [Description("Recording error")]
        RecordingError = 12,

        [Description("Get Asterisk info error")]
        GetAsteriskInfoError = 13,
    }
}