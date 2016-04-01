using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace TestingApp
{
    /// <summary>
    /// Manages the pictures of a directory.
    /// </summary>
    class PictureList
    {
        private int _currentIndex;
        private string directory;
        private BitmapSource[] pictureList = null;
        public int Size { get; private set; }

        /// <summary>
        /// Currend index in the list of images.
        /// </summary>
        public int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                if (value >= 0 && value < pictureList.Length)
                    _currentIndex = value;
            }
        }

        public PictureList(BitmapSource[] pictureList)
        {
            this.pictureList = pictureList;
            Size = pictureList.Length;
            _currentIndex = 0;
            CurrentIndex = 0;
        }

        /// <summary>
        /// Constructs a new Picture List.
        /// </summary>
        /// <param name="directory">Path to the pictures directory</param>
        public PictureList(string directory)
        {
            _currentIndex = 0;
            this.directory = directory;
            string[] files = System.IO.Directory.GetFiles(directory, "*.jpg");
            Size = files.Length;
            pictureList = new BitmapSource[Size];

            //Parallel.For(0, Size, 
            //    i => {
            //        pictureList[i] = new BitmapImage( new Uri(files[i]) ); 
            //    }
            //);

            for (int i = 0; i < Size; ++i )
                pictureList[i] = new BitmapImage(new Uri(files[i])); 

            CurrentIndex = 0;       
        }

        /// <summary>
        /// Returns the image at the current index value on the list.
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource CurrentImage()
        {
            return pictureList[CurrentIndex];
        }

        /// <summary>
        /// Returns the image in the position index of the list. 
        /// </summary>
        /// <param name="index">Index of the required image</param>
        /// <returns>BitmapSource</returns>
        public BitmapSource GetImage(int index)
        {
            if( index >= 0 && index < pictureList.Length)
                return pictureList[index];

            return null;
        }

        /// <summary>
        /// Return the current image on the list and the reference goes to the next. If the current image is the last one, then the first is referenced.
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource Next()
        {
            BitmapSource aux = pictureList[CurrentIndex];

            if (CurrentIndex + 1 >= pictureList.Length)
                CurrentIndex = 0;
            else
                ++CurrentIndex;
            
            return aux;
        }

        /// <summary>
        /// Return the current image on the list and the reference goes to the previuos. If the current image is the first one, then the last is referenced.
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource Prev()
        {
            if (CurrentIndex - 1 <= 0)
                CurrentIndex = pictureList.Length - 1;
            else
                --CurrentIndex;

            return pictureList[CurrentIndex];
        }
    }
}
