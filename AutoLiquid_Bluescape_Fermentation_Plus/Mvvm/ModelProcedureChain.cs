using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Mvvm;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Mvvm
{
    /// <summary>
    /// 程序链 model
    /// </summary>
    public class ModelProcedureChain
    {
        public ModelProcedureChain()
        {
            // 程序链所有程序
            Procedures = new ObservableCollection<ModelSummary>();
        }

        public ObservableCollection<ModelSummary> Procedures { get; set; }
    }

    /// <summary>
    /// 概要信息Model
    /// </summary>
    public class ModelSummary : INotifyPropertyChanged
    {
        // 概要信息
        private string _summary = string.Empty;
        public string Summary
        {
            set => UpdateProperty(ref _summary, value);
            get => _summary;
        }


        private void UpdateProperty<T>(ref T properValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            properValue = newValue;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
