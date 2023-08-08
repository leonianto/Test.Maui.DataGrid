namespace Maui.DataGrid;

using Maui.DataGrid.Extensions;
using Microsoft.Maui.Controls;

public partial class CustomStepper : Grid
{
    public static readonly BindableProperty TagProperty = BindablePropertyExtensions.Create<object>(null);
    public static readonly BindableProperty MaximumProperty = BindablePropertyExtensions.Create<double>(100);
    public static readonly BindableProperty MinimumProperty = BindablePropertyExtensions.Create<double>(0);
    public static readonly BindableProperty ValueProperty = BindablePropertyExtensions.Create<double>(10);
    public static readonly BindableProperty IncrementProperty = BindablePropertyExtensions.Create<double>(10);

    public object Tag
    {
        get => GetValue(TagProperty);
        set { SetValue(TagProperty, value); }
    }

    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set { SetValue(IncrementProperty, value); }
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set { SetValue(MaximumProperty, value); }
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set { SetValue(MinimumProperty, value); }
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set { OnValueChanged(Value, value); SetValue(ValueProperty, value); }
    }

    public event EventHandler<ValueChangedEventArgs> ValueChanged;


    public CustomStepper()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Function fired when Value is changed
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void OnValueChanged(double oldValue, double newValue)
    {
        if (removeBtn != null && addBtn != null)
        {
            if (newValue - Increment <= Minimum)
            {
                removeBtn.IsEnabled = false;
            }
            else
            {
                if (removeBtn != null && !removeBtn.IsEnabled)
                {
                    removeBtn.IsEnabled = true;
                }
            }

            if (newValue + Increment >= Maximum)
            {
                addBtn.IsEnabled = false;
            }
            else
            {
                if (addBtn != null && !addBtn.IsEnabled)
                {
                    addBtn.IsEnabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Function for the decrease the Value of the Stepper by the Increment Value
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void removeBtn_Clicked(object sender, EventArgs e)
    {
        //if value can be decreased
        if (Value - Increment > Minimum)
        {
            if (!addBtn.IsEnabled)
            {
                addBtn.IsEnabled = true;
            }

            ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value, Value - Increment));
        }

    }
    /// <summary>
    /// Function for the increase the Value of the Stepper by the Increment Value
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void addBtn_Clicked(object sender, EventArgs e)
    {
        //if value can be increased
        if (Value + Increment < Maximum)
        {
            if (!removeBtn.IsEnabled)
            {
                removeBtn.IsEnabled = true;
            }

            ValueChanged?.Invoke(this, new ValueChangedEventArgs(Value, Value + Increment));
        }
    }
}
