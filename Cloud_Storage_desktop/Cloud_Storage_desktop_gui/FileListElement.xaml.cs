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

namespace Cloud_Storage_desktop
{
    /// <summary>
    /// Interaction logic for FileListElement.xaml
    /// </summary>
    public partial class FileListElement : UserControl
    {
        public FileListElement()
        {
            InitializeComponent();
            this.Label_FileName.Content = "test";
        }

        public FileListElement(CloudFilesLitemData data)
        {
            InitializeComponent();
            this.Label_FileName.Content = data.name;
        }
    }
}
