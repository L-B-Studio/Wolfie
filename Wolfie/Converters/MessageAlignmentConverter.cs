using System.Globalization;

namespace Wolfie.Converters{

public class MessageAlignmentConverter : IValueConverter
{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string owner = value as string;

            return owner == "Me"
                ? LayoutOptions.End
                : LayoutOptions.Start;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}