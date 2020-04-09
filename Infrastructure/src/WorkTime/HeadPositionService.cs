﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using OpenCvSharp;
using Point = FaceRecognitionDotNet.Point;

namespace Infrastructure.WorkTime
{
    public enum HeadRotation
    {
        Front,
        Left,
        Right,
        Unknown
    }

    public interface IHeadPositionService
    {
        (HeadRotation hRotation, HeadRotation vRotation) GetHeadPosition(Mat frame, Rect face);
    }

    public class HeadPositionService : IHeadPositionService
    {
        private FaceRecognition _recognition;

        public HeadPositionService()
        {
            _recognition = FaceRecognition.Create(".");
        }

        private HeadRotation EstimateHorizontalPose(IDictionary<FacePart, IEnumerable<Point>> landmarks, Rect face, Mat frame)
        {
            var noseTop = landmarks[FacePart.NoseBridge].First();
#if DEBUG
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(noseTop.X, noseTop.Y), new Size2f(2, 2), 0), Scalar.Yellow);
#endif

            Point center = new Point(face.Location.X + face.Width / 2, face.Location.Y + face.Height / 2);

            int dist = (center.X - noseTop.X);

            if (Math.Abs(dist) < 0.05 * face.Width)
            {
                return HeadRotation.Front;
            }

            return dist > 0 ? HeadRotation.Left : HeadRotation.Right;
        }

        private HeadRotation EstimateVerticalPose(IDictionary<FacePart, IEnumerable<Point>> landmarks, Rect face, Mat frame)
        {
            var leftEye = landmarks[FacePart.LeftEyebrow].First();
            var rightEye = landmarks[FacePart.RightEyebrow].Last();

#if DEBUG
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(leftEye.X, leftEye.Y), new Size2f(2, 2), 0), Scalar.Red);
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(rightEye.X, rightEye.Y), new Size2f(2, 2), 0), Scalar.Red);
#endif

            if (leftEye.X - rightEye.X == 0)
            {
                throw new Exception("Cyclope exception");
            }
            double a = (leftEye.Y - rightEye.Y) / (double)(leftEye.X - rightEye.X);
            double angle = Math.Atan(a) * 180.0 / Math.PI;

            //todo to rad
            if (Math.Abs(angle) < 10)
            {
                return HeadRotation.Front;
            }

            return angle > 0 ? HeadRotation.Left : HeadRotation.Right;
        }

        public (HeadRotation hRotation, HeadRotation vRotation) GetHeadPosition(Mat frame, Rect face)
        {
            var bytes = new byte[frame.Rows * frame.Cols * frame.ElemSize()];
            Marshal.Copy(frame.Data, bytes, 0, bytes.Length);
            var img = FaceRecognition.LoadImage(bytes, frame.Rows, frame.Cols, frame.ElemSize());
            var allLandmarks = _recognition.FaceLandmark(img,
                new[] {new Location(face.Left, face.Top, face.Right, face.Bottom),}, PredictorModel.Large);

            var landmarks = allLandmarks.FirstOrDefault();

            if (landmarks == null)
            {
                return (HeadRotation.Unknown, HeadRotation.Unknown);
            }
#if DEBUG
            foreach (var facePart in Enum.GetValues(typeof(FacePart)).Cast<FacePart>())
            {
                foreach (var landmark in landmarks)
                {
                    foreach (var p in landmark.Value.ToArray()) 
                        Cv2.Ellipse(frame, new RotatedRect(new Point2f(p.X, p.Y), new Size2f(2, 2), 0), Scalar.Aqua);
                }
            }
#endif



            return (EstimateHorizontalPose(landmarks, face, frame), EstimateVerticalPose(landmarks, face, frame));
        }
    }
}