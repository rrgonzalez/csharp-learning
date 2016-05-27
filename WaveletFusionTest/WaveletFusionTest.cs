using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WaveletFusion;
using WaveletFusion.Helpers;
using WaveletFusion.Exceptions;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace WaveletFusionTest
{
    [TestClass]
    public class WaveletFusionTest
    {
        [TestMethod]
        public void FuseImagesBitmapSource_WhenOneReferenceIsNull_ShouldThrowNullReferenceException()
        {
            BitmapSource nullBitmapSource = null;
            BitmapSource notNullBitmapSource = new BitmapImage();
            
            // the two arguments null
            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(nullBitmapSource, nullBitmapSource);
            }
            catch (NullReferenceException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.NullImagePtrMessage);
                return;
            }

            // only first argument null
            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(nullBitmapSource, notNullBitmapSource);
            }
            catch (NullReferenceException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.NullImagePtrMessage);
                return;
            }

            // only second argument null
            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(notNullBitmapSource, nullBitmapSource);
            }
            catch (NullReferenceException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.NullImagePtrMessage);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }

        [TestMethod]
        public void FuseImagesImagePtr_WhenOneReferenceIsNull_ShouldThrowNullReferenceException()
        {
            ImagePtr nullImagePtr = null;
            ImagePtr notNullImagePtr = new ImagePtr(512, 512, false);

            // the two arguments null
            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(nullImagePtr, nullImagePtr);
            }
            catch(NullReferenceException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.NullImagePtrMessage);
                return;
            }

            // only first argument null
            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(nullImagePtr, notNullImagePtr);
            }
            catch (NullReferenceException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.NullImagePtrMessage);
                return;
            }

            // only second argument null
            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(notNullImagePtr, nullImagePtr);
            }
            catch (NullReferenceException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.NullImagePtrMessage);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }

        [TestMethod]
        public void FuseImages_WhenImageSizeIsInvalid_ShouldThrowInvalidImageResolutionException()
        {
            ImagePtr image512 = new ImagePtr(512, 512, false);
            ImagePtr notSquare = new ImagePtr(512, 128, false);
            ImagePtr badSizeImage = new ImagePtr(438, 38, false);

            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(image512, image512);
            }
            catch (InvalidImageResolutionException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.InvalidImageSizeMessage);
                return;
            }

            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(image512, notSquare);
            }
            catch (InvalidImageResolutionException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.InvalidImageSizeMessage);
                return;
            }

            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(image512, badSizeImage);
            }
            catch (InvalidImageResolutionException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.InvalidImageSizeMessage);
                return;
            }

            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(notSquare, badSizeImage);
            }
            catch (InvalidImageResolutionException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.InvalidImageSizeMessage);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }

        [TestMethod]
        public void FuseImages_WhenImageSpectralResolutionIsInvalid_ShouldThrowInvalidImageResolutionException()
        {
            ImagePtr validSpectResolutionImage = new ImagePtr(512, 512, PixelFormats.Gray16);
            ImagePtr invalidSpectResolutionImage = new ImagePtr(256, 256, PixelFormats.Cmyk32);

            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(validSpectResolutionImage, invalidSpectResolutionImage);
            }
            catch (InvalidImageResolutionException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.InvalidImageSpectralResolutionMessage);
                return;
            }

            try
            {
                WaveletFusionLib.WaveletFusion.FuseImages(invalidSpectResolutionImage, invalidSpectResolutionImage);
            }
            catch (InvalidImageResolutionException e)
            {
                StringAssert.Equals(e.Message, WaveletFusionLib.WaveletFusion.InvalidImageSpectralResolutionMessage);
                return;
            }

            Assert.Fail("No exception was thrown.");
        }
    }
}
