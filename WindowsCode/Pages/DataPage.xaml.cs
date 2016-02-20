﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsCode.Classes;
using Windows.UI;
using System.Text;
using System.Diagnostics;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsCode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DataPage : Page
    {
        StringBuilder dataBuilder = new StringBuilder();
        public DataPage()
        {
            this.InitializeComponent();

            DataState.Data.CollectionChanged -= Data_CollectionChanged;
            DataState.Data.CollectionChanged += Data_CollectionChanged;
        }

        private async void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            dataBuilder = new StringBuilder();
            (from data in e.NewItems.Cast<CSVData>() where data != null select data.RawData + "\n").ForEach((raw) => dataBuilder.Append(raw));
            FileIO.AppendTextAsync((await StorageApplicationPermissions.FutureAccessList.GetFileAsync(DataState.OutputFileToken)), dataBuilder.ToString()).AsTask().Wait();
            DataBlock.Text += dataBuilder.ToString();
        }
    }
}
