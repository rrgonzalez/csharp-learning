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
using System.IO;

using WaveletFusionLib;
using System.Windows.Forms;

namespace TestingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource targetImage;
        private PictureList targetSeries = null;
        private PictureList objectSeries = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonOpenImg_Click(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPG Files (*.jpg)|*.jpg";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                BitmapSource tempBitmap = new BitmapImage(new Uri(dlg.FileName));

                if ((((int)tempBitmap.Width & ((int)tempBitmap.Width - 1)) != 0) ||
                     (((int)tempBitmap.Height & ((int)tempBitmap.Height - 1)) != 0))
                {
                    System.Windows.MessageBox.Show(this, "Image dimensions must be power of 2", "Error!");
                    return;
                }

                imageRender.Source = this.targetImage = new BitmapImage(new Uri(dlg.FileName));
            }
        }

        private void buttonApplyDWT_Click(object sender, RoutedEventArgs e)
        {
            WaveletFusionLib.WaveletFusion.FusionImages(targetImage, targetImage);
            imageRender.Source = WaveletFusionLib.WaveletFusion.targetImage;
        }

        private void buttonApplyIDWT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonLoadTargetSeries_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDlg = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK )
            {
                targetSeries = new PictureList(folderBrowserDlg.SelectedPath);                
            }
        }

        private void buttonLoadObjectSeries_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
