using System.Globalization;

namespace Wolfie.Converters{
    public class MessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string owner = value as string;

            return owner == "Me"
                ? Colors.LightBlue
                : Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}