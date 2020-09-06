using System.Globalization;
using demo.FunctionalExtensions;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.Dtos.Sms;

namespace demo.DemoGateway.Infrastructure.Parsers.Sms
{
    /// <summary>
    /// Парсер для конкретных полей СМС
    /// </summary>
    public class SmsFieldParser
    {
        /// <summary>
        /// Парсер, который возвращает переданное в него значение
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public Result<string> EmptyParser(string field)
        {
            return Result.Success(field);
        }

        /// <summary>
        /// Парсинг int из строки
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public Result<int?> IntParser(string field)
        {
            if (!int.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return Result.Failure<int?>(ErrorCodes.ParsingError);
            }

            int? nullableResult = result; 
            return Result.Success(nullableResult);
        }

        /// <summary>
        /// Парсинг double из строки
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private Result<double> DoubleParser(string field)
        {
            if (!double.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return Result.Failure<double>(ErrorCodes.ParsingError);
            }

            return Result.Success(result);
        }

        /// <summary>
        /// Парсинг строки, содержащей данные о позиции
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public Result<SmsPosition> PositionParser(string field)
        {
            var splitted = field.Split(" ");
            if (splitted.Length != 2)
            {
                return Result.Failure<SmsPosition>(ErrorCodes.ParsingError);
            }

            var latitudeStr = splitted[0];
            var latitudeResult = DoubleParser(latitudeStr);
            if (latitudeResult.IsFailure)
            {
                return Result.Failure<SmsPosition>(ErrorCodes.ParsingError);
            }

            var longitudeStr = splitted[1];
            var longitudeResult = DoubleParser(longitudeStr);
            if (latitudeResult.IsFailure)
            {
                return Result.Failure<SmsPosition>(ErrorCodes.ParsingError);
            }

            var position = new SmsPosition
            {
                Latitude = latitudeResult.Value,
                Longitude = longitudeResult.Value,
            };

            return Result.Success(position);
        }
    }
}