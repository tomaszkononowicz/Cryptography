using Cryptography.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Cryptography.Converters
{
    class RadioButtonToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return ApplicationModeEnum.Encrypt;
            
            bool state = (bool)value;
            if (state) return System.Convert.ChangeType(parameter, targetType);
            else return 0;
        }
    }
}
