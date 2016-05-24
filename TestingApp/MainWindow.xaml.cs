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
        private PictureList resultSeries = null;

        private const string paletteFilter = "LUT/Hotiron.lut";

        private bool[] fused = null;
        //private PictureList playingSeries = null;

        public MainWindow()
        {
            InitializeComponent();
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
            //playingSeries = targetSeries;

            fused = new bool[targetSeries.Size];
            resultSeries = new PictureList(fused.Length);
            for (int i = 0; i < fused.Length; ++i)
                fused[i] = false;

            if (scrollPlayingSeries.Value != 0)
                targetSeries.CurrentIndex = (int)scrollPlayingSeries.Value;
            else
                scrollPlayingSeries.Value = 0;

            imageRender.Source = targetSeries.CurrentImage();
            scrollPlayingSeries.Maximum = targetSeries.Size - 1;
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
            objectSeries.Reverse();
            //playingSeries = objectSeries;

            fused = new bool[objectSeries.Size];
            resultSeries = new PictureList(fused.Length);
            for (int i = 0; i < fused.Length; ++i)
                fused[i] = false;

            if (scrollPlayingSeries.Value != 0)
                objectSeries.CurrentIndex = (int)scrollPlayingSeries.Value;
            else
                scrollPlayingSeries.Value = 0;

            imageRenderObject.Source = objectSeries.CurrentImage();

            scrollPlayingSeries.Maximum = objectSeries.Size - 1;
        }

        private void scrollPlayingSeries_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            targetSeries.CurrentIndex = (int)scrollPlayingSeries.Value;
            objectSeries.CurrentIndex = (int)scrollPlayingSeries.Value;
            resultSeries.CurrentIndex = (int)scrollPlayingSeries.Value;
            imageRender.Source = targetSeries.CurrentImage();
            imageRenderObject.Source = objectSeries.CurrentImage();
        }

        private void buttonFuse_Click(object sender, RoutedEventArgs e)
        {
            if (targetSeries == null || objectSeries == null || targetSeries.Size != objectSeries.Size )
                return;

            if( fused[targetSeries.CurrentIndex] )
            {
                imageRender.Source = resultSeries.CurrentImage();
                return;
            }

            int N = targetSeries.Size;
            BitmapSource aux;
            //BitmapSource[] bmpArray = new BitmapSource[N];
            //for (int i = 0; i < N; ++i)
            //{
            //    aux = WaveletFusionLib.WaveletFusion.FusionImages(targetSeries.Next(), objectSeries.Next());
            //    //bmpArray[i] = ConvertBitmapSourceToBitmapImage(aux);
            //    bmpArray[i] = aux;
            //}

            aux = WaveletFusionLib.WaveletFusion.FuseImages(targetSeries.CurrentImage(), objectSeries.CurrentImage(), paletteFilter);

            resultSeries.InsertPicture(targetSeries.CurrentIndex, aux);
            fused[targetSeries.CurrentIndex] = true;

            imageRender.Source = aux;

            //playingSeries = resultSeries = new PictureList(bmpArray);
        }

        private BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (targetSeries.CurrentIndex <= 0 )
                return;

            --scrollPlayingSeries.Value;

            //--targetSeries.CurrentIndex;
            //--objectSeries.CurrentIndex;
            //--resultSeries.CurrentIndex;

            int index = resultSeries.CurrentIndex;
            if (fused[index])
                imageRender.Source = resultSeries.CurrentImage();
            else
            {
                BitmapSource aux;

                aux = WaveletFusionLib.WaveletFusion.FuseImages(targetSeries.CurrentImage(), objectSeries.CurrentImage(), paletteFilter);

                resultSeries.InsertPicture(targetSeries.CurrentIndex, aux);
                fused[targetSeries.CurrentIndex] = true;

                imageRender.Source = aux;
            }
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (targetSeries.CurrentIndex + 1 >= targetSeries.Size)
                return ;

            ++scrollPlayingSeries.Value;

            //++targetSeries.CurrentIndex;
            //++objectSeries.CurrentIndex;
            //++resultSeries.CurrentIndex;

            int index = resultSeries.CurrentIndex;
            if (fused[index])
                imageRender.Source = resultSeries.CurrentImage();
            else
            {
                BitmapSource aux;

                aux = WaveletFusionLib.WaveletFusion.FuseImages(targetSeries.CurrentImage(), objectSeries.CurrentImage(), paletteFilter);

                resultSeries.InsertPicture(targetSeries.CurrentIndex, aux);
                fused[targetSeries.CurrentIndex] = true;

                imageRender.Source = aux;
            }
        }
    }
}
