namespace Maui.DataGrid;

using System.Globalization;

internal sealed class BoolToSelectionModeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? SelectionMode.Multiple : SelectionMode.None;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
