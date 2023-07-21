namespace Maui.DataGrid;

using Microsoft.Maui.Controls;

public partial class CustomStepper : Stepper
{
    public static readonly BindableProperty TagProperty = BindableProperty.CreateAttached(nameof(Tag), typeof(object), typeof(CustomStepper), false);

    public object Tag
    {
        get => GetValue(TagProperty);
        set { SetValue(TagProperty, value); }
    }

    public CustomStepper()
    {
        InitializeComponent();
    }
}
