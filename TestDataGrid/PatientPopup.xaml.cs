namespace TestDataGrid;

using System;
using System.Globalization;
using System.Xml.Linq;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Platform;

public partial class PatientPopup : Popup
{

    private IList<object> _Patients;
    private IList<IView> _GridChildrens;

    public PatientPopup(SelectionMode selectionMode, IList<object> selectedItems)
	{
        _GridChildrens = new List<IView>();
        _Patients = selectedItems;
        InitializeComponent();
        _InitPatientPopup(selectionMode);

	}

    private void _InitPatientPopup(SelectionMode selectionMode)
    {
       if(selectionMode == SelectionMode.Single)
        {
            _GridChildrens.Clear();

            Label patientName = new Label()
            {

                Text = ((Patient)_Patients[0]).Name,

            };

            Label patientId = new Label()
            {

                Text = ((Patient)_Patients[0]).Id.ToString(CultureInfo.InvariantCulture),

            };

           MainPanel.Children.Add(patientName);
            MainPanel.Children.Add(patientId);

        } else if (selectionMode == SelectionMode.Multiple)
        {


        }


    }
}

