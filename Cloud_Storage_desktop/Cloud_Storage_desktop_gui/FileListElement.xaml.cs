using System.Windows.Controls;

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
