using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.Dtos.Sms;

namespace demo.DemoGateway.Infrastructure.Parsers.Sms
{
    /// <summary>
    /// Парсинг СМС
    /// </summary>
    public class SmsParser
    {
        private const string XmlPositionTagName = "gml:pos";
        private const string XmlTimestampTagName = "dm:timestamp";
        private const string XmlRadiusTagName = "gs:radius";
        private const string XmlInnerRadiusTagName = "gs:innerRadius";
        private const string XmlOuterRadiusTagName = "gs:outerRadius";
        private const string XmlStartAngleTagName = "gs:startAngle";
        private const string XmlOpeningAngleTagName = "gs:openingAngle";

        private const string SmsXmlRegexPattern = @"^\s*$(\n|\r\n)(?<xmlcontent><\?xml((\n|\r|.)*)>)(\n|\r\n)--.*";
        private const string SmsXmlRegexPatternGroupName = "xmlcontent";

        private const string SmsTextRegexPattern = @"--(\n|.)*^\s*$(?<text>(\n|.)*)--.*--";
        private const string SmsTextRegexPatternGroupName = "text";

        private readonly ILogger _logger;
        private readonly SmsFieldParser _fieldParser;

        /// <inheritdoc />
        public SmsParser(ILogger logger, SmsFieldParser fieldParser)
        {
            _logger = logger;
            _fieldParser = fieldParser;
        }

        /// <summary>
        /// Парсинг текста из SMS Body
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Result<string> GetSmsText(string body)
        {
            var result = Parse(body, SmsTextRegexPattern, SmsTextRegexPatternGroupName);
            return result;
        }

        /// <summary>
        /// Получение XML из SMS Body
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Result<SmsMetadata> GetMetadata(string body)
        {
            var result = Parse(body, SmsXmlRegexPattern, SmsXmlRegexPatternGroupName);
            if (result.IsFailure)
            {
                _logger.Warning($"Error getting metadata from sms body. {result.ErrorMessage}");
                return Result.Failure<SmsMetadata>(ErrorCodes.ParsingError);
            }

            var xml = result.Value;

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;


            var timestamp = GetMetadataField(root, XmlTimestampTagName, _fieldParser.EmptyParser);
            var position = GetMetadataField(root, XmlPositionTagName, _fieldParser.PositionParser);
            var radius = GetMetadataField(root, XmlRadiusTagName, _fieldParser.IntParser);
            var innerRadius = GetMetadataField(root, XmlInnerRadiusTagName, _fieldParser.IntParser);
            var outerRadius = GetMetadataField(root, XmlOuterRadiusTagName, _fieldParser.IntParser);
            var startAngle = GetMetadataField(root, XmlStartAngleTagName, _fieldParser.IntParser);
            var openingAngle = GetMetadataField(root, XmlOpeningAngleTagName, _fieldParser.IntParser);

            var data = new SmsMetadata()
            {
                Timestamp = timestamp,
                Position = position,
                Radius = radius,
                OuterRadius = outerRadius,
                InnerRadius = innerRadius,
                StartAngle = startAngle,
                OpeningAngle = openingAngle
            };

            return Result.Success(data);
        }

        private T GetMetadataField<T>(XmlElement xmlRoot, string xmlTagName, Func<string, Result<T>> fieldParser)
        {
            var rawFieldResult = GetDataFromXml(xmlRoot, xmlTagName);

            if (rawFieldResult.IsFailure)
            {
                _logger.Information(
                    $"SMS Parser. Missing xml tag {xmlTagName} in metadata sms body. {rawFieldResult.ErrorMessage}");
                return default(T);
            }

            var parsedDataResult = fieldParser.Invoke(rawFieldResult.Value);

            if (parsedDataResult.IsFailure)
            {
                _logger.Warning(
                    $"SMS Parser. Data parsing error: {rawFieldResult.Value}, received from xml tag {xmlTagName}. {parsedDataResult.ErrorMessage}");
                return default(T);
            }

            return parsedDataResult.Value;
        }


        private Result<string> GetDataFromXml(XmlElement node, string tagName)
        {
            var firstElement = node.GetElementsByTagName(tagName).Cast<XmlNode>().FirstOrDefault();
            if (firstElement == null)
            {
               _logger.Warning($"Node not found '{tagName}' in xml"); 
                return Result.Failure<string>(ErrorCodes.ParsingError);
            }

            var text = firstElement.InnerText;
            return Result.Success(text);
        }

        private Result<string> Parse(string body, string pattern, string matchGroupName)
        {
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
            var regex = new Regex(pattern, regexOptions);

            var results = regex.Matches(body);

            var match = results.FirstOrDefault();
            if (match == null)
            {
               _logger.Warning($"No pattern matching {pattern}");
                return Result.Failure<string>(ErrorCodes.ParsingError);
            }

            var group = match.Groups[matchGroupName];
            if (group == null)
            {
               _logger.Warning($"Missing group '{matchGroupName}' as a result of regular search");
                return Result.Failure<string>(ErrorCodes.ParsingError);

            }

            var resultText = group.Value.Trim('\n', '\r');

            return Result.Success(resultText);
        }
    }
}