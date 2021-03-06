﻿using FaceRecognitionDotNet;
using OpenCvSharp;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace WMAlghorithm
{
    public interface IDnFaceRecognition
    {
        bool CompareFaces(Mat photo1, FaceEncodingData? faceEncoding1, Mat photo2, FaceEncodingData? faceEncoding2);
        FaceEncodingData? GetFaceEncodings(Mat photo);
    }

    public class DnFaceRecognition : IDnFaceRecognition
    {
        private const double RecognitionThreshold = 0.55;

        private Image LoadImage(Mat photo)
        {
            var bytes = new byte[photo.Rows * photo.Cols * photo.ElemSize()];
            Marshal.Copy(photo.Data, bytes, 0, bytes.Length);

            var img = FaceRecognition.LoadImage(bytes, photo.Rows, photo.Cols, photo.ElemSize());
            return img;
        }

        private FaceEncodingData? InternalGetFaceEncoding(Image img)
        {
            var imgEncodings = SharedFaceRecognitionModel.FaceEncodingsSync(img, null, model: PredictorModel.Small);

            if (imgEncodings.Count != 1)
            {
                return null;
            }

            return new FaceEncodingData(imgEncodings.First());
        }

        private double InternalCompareFaces(Mat photo1, FaceEncodingData? faceEncoding1, Mat photo2, FaceEncodingData? faceEncoding2)
        {
            using var img1 = LoadImage(photo1);


            if (faceEncoding1 == null)
            {
                faceEncoding1 = InternalGetFaceEncoding(img1);
                if (faceEncoding1 == null)
                {
                    return double.MaxValue;
                }
            }

            using var img2 = LoadImage(photo2);

            if (faceEncoding2 == null)
            {
                faceEncoding2 = InternalGetFaceEncoding(img2);
                if (faceEncoding2 == null)
                {
                    return double.MaxValue;
                }
            }

            var distance = FaceRecognition.FaceDistance(faceEncoding1.Value, faceEncoding2.Value);
            Debug.WriteLine($"faces dist {distance}");
            return distance;
        }

        public bool CompareFaces(Mat photo1, FaceEncodingData? faceEncoding1, Mat photo2, FaceEncodingData? faceEncoding2)
        {
            return InternalCompareFaces(photo1, faceEncoding1, photo2, faceEncoding2) < RecognitionThreshold;
        }

        public FaceEncodingData? GetFaceEncodings(Mat photo)
        {
            using var img1 = LoadImage(photo);
            return InternalGetFaceEncoding(img1);
        }
    }
}