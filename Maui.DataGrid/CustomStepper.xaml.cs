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
        set { /*OnValueChanged(Value, value);*/ SetValue(ValueProperty, value); }
    }

    public event EventHandler<ValueChangedEventArgs> ValueChanged;


    public CustomStepper()
    {
        InitializeComponent();
    }

    /*/// <summary>
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
    }*/

    /// <summary>
    /// Function for the decrease the Value of the Stepper by the Increment Value
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _RemoveBtn_Clicked(object sender, EventArgs e)
    {
        DataGridColumn dataGridColumn = (DataGridColumn)((Button)sender).BindingContext;

        // Decrease the value by 10, but ensure it doesn't go below the minimum of 92
        dataGridColumn.WidthCol -= 10;
        if (dataGridColumn.WidthCol < Minimum)
        {
            dataGridColumn.WidthCol = Minimum;
        }

        entryWidth.Text = dataGridColumn.WidthCol.ToString();
    }
    /// <summary>
    /// Function for the increase the Value of the Stepper by the Increment Value
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _AddBtn_Clicked(object sender, EventArgs e)
    {
        DataGridColumn dataGridColumn = (DataGridColumn)((Button)sender).BindingContext;

        // Increase the value by 10, but ensure it doesn't exceed the maximum of 500
        dataGridColumn.WidthCol += 10;
        if (dataGridColumn.WidthCol > Maximum)
        {
            dataGridColumn.WidthCol = Maximum;
        }

        entryWidth.Text = dataGridColumn.WidthCol.ToString();
    }

    /// <summary>
    /// Set the new widthCol value after user press return on the entry button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _EntryUnfocus(object sender, EventArgs e)
    {
        double newValue = Double.Parse(((Entry)sender).Text);
        DataGridColumn dataGridColumn = (DataGridColumn)((Entry)sender).BindingContext;

        if (newValue >= Minimum && newValue <= Maximum)
        {
            dataGridColumn.WidthCol = newValue;
        }
        else
        {
            if(newValue < Minimum)
            {
                dataGridColumn.WidthCol = Minimum;
            }
           else if(newValue > Maximum)
            {
                dataGridColumn.WidthCol = Maximum;
            }
        }

    }



}
