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
            int integer = (int)value;
            if (integer == int.Parse(parameter.ToString()))
                return true;
            else
                return false;//jak ustawiam z programu
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            if (state) return int.Parse(parameter.ToString()) ;
            else return 0;
        }
    }
}
