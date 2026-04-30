using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.Truv.Business
{
    public class BaseClass : VOESystem.Data.Business.BusinessBase
    {
        
        public bool TruvSendBorrowerEmailFromVOESystem = Boolean.Parse(ConfigurationManager.AppSettings["TruvSendBorrowerEmailFromVOESystem"].ToString());
        
        public enum VerificationType
        {
            Current,
            Prior,
            All
        }

        public enum OrderType
        {
            Verbal = 1,
            Written = 2
        }


    }

    //Extension methods must be defined in a static class
    public static class StringExtension
    {

        public static string CleanCharsForCompare(this string str)
        {
            if (str == null) { return null; }

            str = str.Replace("**", String.Empty);
            str = str.Replace("\r", String.Empty);
            str = str.Replace("\n", String.Empty);
            str = str.Replace(" ", String.Empty);
            return str;
        }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((System.ComponentModel.DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }
    }
}
