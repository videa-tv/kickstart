using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.SqlServer.SnakeCase.App
{
    public class SnakeCaseTranslator
    {
        public static string MySnakeCase(string stringIn)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < stringIn.Length; i++)
            {
                var c = stringIn[i];

                if (i > 0 && i < stringIn.Length - 1)
                {
                    char previousX = stringIn[i - 1];
                    if (char.IsUpper(c) &&
                        (!char.IsUpper(previousX) || //don't insert between acronyms
                        (i < stringIn.Length - 1 && char.IsLower(stringIn[i + 1]))))
                    {
                        if (i == 0 || stringIn[i - 1] != '_') //don't double up -> __
                            sb.Append('_');
                    }
                }

                sb.Append(c.ToString().ToLower());
            }
            return sb.ToString();

        }

        public static string SnakeCase(string stringIn)
        {
            if (stringIn == null)
                return stringIn;

            if (stringIn.StartsWith("@"))
            {
                return "@" + ConvertToSnakeCase(stringIn.Substring(1, stringIn.Length - 1));
            }
            else if (stringIn.StartsWith("#"))
            {
                return "#" + ConvertToSnakeCase(stringIn.Substring(1, stringIn.Length - 1));
            }
            else
                return ConvertToSnakeCase(stringIn);
        }

        /// <summary>
        /// Converts a string to its snake_case equivalent.
        /// </summary>
        /// <remarks>
        /// Code borrowed from Ngpsql v4
        /// https://github.com/npgsql/npgsql/blob/dev/src/Npgsql/NameTranslation/NpgsqlSnakeCaseNameTranslator.cs
        /// which was
        /// Code borrowed from Newtonsoft.Json.
        /// See https://github.com/JamesNK/Newtonsoft.Json/blob/f012ba857f36fe75b1294a210b9104130a4db4d5/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L200-L276.
        /// </remarks>
        /// <param name="value">The value to convert.</param>
        public static string ConvertToSnakeCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var sb = new StringBuilder();
            var state = SnakeCaseState.Start;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == ' ')
                {
                    if (state != SnakeCaseState.Start)
                        state = SnakeCaseState.NewWord;
                }
                else if (char.IsUpper(value[i]))
                {
                    switch (state)
                    {
                        case SnakeCaseState.Upper:
                            var hasNext = (i + 1 < value.Length);
                            if (i > 0 && hasNext)
                            {
                                var nextChar = value[i + 1];
                                if (!char.IsUpper(nextChar) && !char.IsDigit(nextChar) && nextChar != '_')
                                {
                                    sb.Append('_');
                                }
                            }
                            break;
                        case SnakeCaseState.Lower:
                        case SnakeCaseState.NewWord:
                            sb.Append('_');
                            break;
                    }

                    sb.Append(char.ToLowerInvariant(value[i]));
                    state = SnakeCaseState.Upper;
                }
                else if (value[i] == '_')
                {
                    sb.Append('_');
                    state = SnakeCaseState.Start;
                }
                else
                {
                    if (state == SnakeCaseState.NewWord)
                        sb.Append('_');

                    sb.Append(value[i]);
                    state = SnakeCaseState.Lower;
                }
            }

            return sb.ToString();
        }

        enum SnakeCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        public string TranslateTypeName(string clrName)
        {
            return ClrToDatabaseName(clrName);
        }

        public string TranslateMemberName(string clrName)
        {
            return ClrToDatabaseName(clrName);
        }

        static string ClrToDatabaseName(string clrName)
        {
            return SnakeCase(clrName);
        }
    }
    public static class ExtenstionMethods
    {
        public static string ToSnakeCase(this string str)
        {
            return SnakeCaseTranslator.SnakeCase(str);
        }
    }
}
