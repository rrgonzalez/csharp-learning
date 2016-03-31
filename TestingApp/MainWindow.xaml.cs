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
        //private BitmapSource targetImage;
        private PictureList targetSeries = null;
        private PictureList objectSeries = null;
        private PictureList playingSeries = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonApplyDWT_Click(object sender, RoutedEventArgs e)
        {
            //WaveletFusionLib.WaveletFusion.FusionImages(targetImage, targetImage);
            //imageRender.Source = WaveletFusionLib.WaveletFusion.targetImage;
        }

        private void buttonApplyIDWT_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonLoadTargetSeries_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDlg = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDlg.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK )
            {
                return;             
            }

            targetSeries = new PictureList(folderBrowserDlg.SelectedPath);  
            playingSeries = targetSeries;

            scrollPlayingSeries.Value = 0;
            imageRender.Source = playingSeries.CurrentImage();
            scrollPlayingSeries.Maximum = playingSeries.Size - 1;
        }

        private void buttonLoadObjectSeries_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDlg = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDlg.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            objectSeries = new PictureList(folderBrowserDlg.SelectedPath);
            playingSeries = objectSeries;

            scrollPlayingSeries.Value = 0;
            imageRender.Source = playingSeries.CurrentImage();
            scrollPlayingSeries.Maximum = playingSeries.Size - 1;
        }

        private void scrollPlayingSeries_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            playingSeries.CurrentIndex = (int)scrollPlayingSeries.Value;
            imageRender.Source = playingSeries.CurrentImage();
        }
    }
}
