using System;
using System.Collections.Generic;
using System.Text;

namespace PrismWpf.Mvvm.ViewModelBases
{
    public static class MainThreadRecordHelper
    {
        /// <summary>
        /// Record the id of main thread (UI thread)
        /// Should override the CreateShell Method in App.xaml.cs, and add following codes:
        /// MainThreadRecordHelper.MaintThreadID = Thread.CurrentThread.ManagedThreadId;
        /// </summary>
        public static int MaintThreadID { get; set; }
    }
}
