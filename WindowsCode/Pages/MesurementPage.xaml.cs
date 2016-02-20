using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static WindowsCode.Classes.Utils;
using WindowsCode.Classes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsCode.Pages
{

    public sealed partial class MesurementsPage : Page
    {
        private ObservableCollection<RecentItem> RecentItems { get { return MesurementState.RecentItems; } }

        public MesurementsPage()
        {
            this.InitializeComponent();
            if(RecentItems.Count < 1)
                RecentItem.AddItem(new AddRecentItem());
        }

    }

}


