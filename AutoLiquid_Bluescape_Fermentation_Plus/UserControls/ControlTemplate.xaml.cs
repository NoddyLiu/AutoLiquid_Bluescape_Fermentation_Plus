using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// Interaction logic for ControlTemplate.xaml
    /// </summary>
    public partial class ControlTemplate : UserControl
    {
        private int mTemplateIndex;

        public ControlTemplate(int templateIndex)
        {
            InitializeComponent();

            this.mTemplateIndex = templateIndex;

            InitWidget();
        }

        private void InitWidget()
        {
            this.TextBlockTemplateName.Text = (string) this.FindResource("Template") + (this.mTemplateIndex + 1);
        }
    }
}
