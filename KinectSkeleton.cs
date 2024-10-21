//------------------------------------------------------------------------------
// <copyright file="KinectSkeleton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.WpfViewers
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.IO;
    using System;
    using MySql.Data.MySqlClient;

    /// <summary>
    /// This control is used to render a player's skeleton.
    /// If the ClipToBounds is set to "false", it will be allowed to overdraw
    /// it's bounds.
    /// </summary>
    public class KinectSkeleton : Control
    {

        static StreamWriter fileWriter = new StreamWriter("average.csv");
        static StreamWriter allFileWriter = new StreamWriter("averageeveryan.csv");
        static StreamWriter pointFileWriter = new StreamWriter("averagepoint.csv");
        private int frameCount = 0;
        //평균을 위한 합계
        private double averageshoulderRightAngleTotal = 0.0;
        private double averageshoulderLeftAngleTotal = 0.0;
        private double averageKneeLeftAngleTotal = 0.0;
        private double averageKneeRightAngleTotal = 0.0;
        private double averageLegLeftAngleTotal = 0.0;
        private double averageLegRightAngleTotal = 0.0;
        //각도들
        private double averageshoulderRightAngle = 0.0;
        private double averageshoulderLeftAngle = 0.0;
        private double averageKneeLeftAngle = 0.0;
        private double averageKneeRightAngle = 0.0;
        private double averageLegLeftAngle = 0.0;
        private double averageLegRightAngle = 0.0;
        // csv파일에 삽입할 값
        private int wantedFameCount = 30;
        private string averageshoulderRightAngle1 = "";
        private string averageshoulderLeftAngle1 = "";
        private string averageKneeLeftAngle1 = "";
        private string averageKneeRightAngle1 = "";
        private string averageLegLeftAngle1 = "";
        private string averageLegRightAngle1 = "";

        //private static string connectingString = "server=localhost;port=3306;database=fff;uid=root;password=1234;";
        //MySqlConnection connection = new MySqlConnection(connectingString);

        public static readonly DependencyProperty ShowClippedEdgesProperty =
            DependencyProperty.Register("ShowClippedEdges", typeof(bool), typeof(KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowJointsProperty =
            DependencyProperty.Register("ShowJoints", typeof(bool), typeof(KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowBonesProperty =
            DependencyProperty.Register("ShowBones", typeof(bool), typeof(KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowCenterProperty =
            DependencyProperty.Register("ShowCenter", typeof(bool), typeof(KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SkeletonProperty =
            DependencyProperty.Register(
                "Skeleton",
                typeof(Skeleton),
                typeof(KinectSkeleton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty JointMappingsProperty =
            DependencyProperty.Register(
                "JointMappings",
                typeof(Dictionary<JointType, JointMapping>),
                typeof(KinectSkeleton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register(
                "Center",
                typeof(Point),
                typeof(KinectSkeleton),
                new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.Register(
                "ScaleFactor",
                typeof(double),
                typeof(KinectSkeleton),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double TrackedBoneThickness = 6;
        private const double InferredBoneThickness = 1;
        private const double ClipBoundsThickness = 10;

        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, TrackedBoneThickness);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, InferredBoneThickness);

        private readonly Brush bottomClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 0), new Point(0, 1));

        private readonly Brush topClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 1), new Point(0, 0));

        private readonly Brush leftClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(1, 0), new Point(0, 0));

        private readonly Brush rightClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 0), new Point(1, 0));

        public bool ShowClippedEdges
        {
            get { return (bool)GetValue(ShowClippedEdgesProperty); }
            set { SetValue(ShowClippedEdgesProperty, value); }
        }

        public bool ShowJoints
        {
            get { return (bool)GetValue(ShowJointsProperty); }
            set { SetValue(ShowJointsProperty, value); }
        }

        public bool ShowBones
        {
            get { return (bool)GetValue(ShowBonesProperty); }
            set { SetValue(ShowBonesProperty, value); }
        }

        public bool ShowCenter
        {
            get { return (bool)GetValue(ShowCenterProperty); }
            set { SetValue(ShowCenterProperty, value); }
        }

        public Skeleton Skeleton
        {
            get { return (Skeleton)GetValue(SkeletonProperty); }
            set { SetValue(SkeletonProperty, value); }
        }

        public Dictionary<JointType, JointMapping> JointMappings
        {
            get { return (Dictionary<JointType, JointMapping>)GetValue(JointMappingsProperty); }
            set { SetValue(JointMappingsProperty, value); }
        }

        public Point Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public double ScaleFactor
        {
            get { return (double)GetValue(ScaleFactorProperty); }
            set { SetValue(ScaleFactorProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return arrangeBounds;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var currentSkeleton = this.Skeleton;

            // Don't render if we don't have a skeleton, or it isn't tracked
            if (drawingContext == null || currentSkeleton == null || currentSkeleton.TrackingState == SkeletonTrackingState.NotTracked)
            {
                return;
            }

            // Displays a gradient near the edge of the display where the skeleton is leaving the screen
            this.RenderClippedEdges(drawingContext);

            switch (currentSkeleton.TrackingState)
            {
                case SkeletonTrackingState.PositionOnly:
                    if (this.ShowCenter)
                    {
                        drawingContext.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.Center,
                            BodyCenterThickness * this.ScaleFactor,
                            BodyCenterThickness * this.ScaleFactor);
                    }

                    break;
                case SkeletonTrackingState.Tracked:
                    this.DrawBonesAndJoints(drawingContext);
                    break;
            }
        }

        private void RenderClippedEdges(DrawingContext drawingContext)
        {
            var currentSkeleton = this.Skeleton;

            if (!this.ShowClippedEdges ||
                currentSkeleton.ClippedEdges.Equals(FrameEdges.None))
            {
                return;
            }

            double scaledThickness = ClipBoundsThickness * this.ScaleFactor;
            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    this.bottomClipBrush,
                    null,
                    new Rect(0, this.RenderSize.Height - scaledThickness, this.RenderSize.Width, scaledThickness));
            }

            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    this.topClipBrush,
                    null,
                    new Rect(0, 0, this.RenderSize.Width, scaledThickness));
            }

            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    this.leftClipBrush,
                    null,
                    new Rect(0, 0, scaledThickness, this.RenderSize.Height));
            }

            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    this.rightClipBrush,
                    null,
                    new Rect(this.RenderSize.Width - scaledThickness, 0, scaledThickness, this.RenderSize.Height));
            }
        }

        private void DrawBonesAndJoints(DrawingContext drawingContext)
        {
            if (this.ShowBones)
            {
                // Render Torso
                this.DrawBone(drawingContext, JointType.Head, JointType.ShoulderCenter);
                this.DrawBone(drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
                this.DrawBone(drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
                this.DrawBone(drawingContext, JointType.ShoulderCenter, JointType.Spine);
                this.DrawBone(drawingContext, JointType.Spine, JointType.HipCenter);
                this.DrawBone(drawingContext, JointType.HipCenter, JointType.HipLeft);
                this.DrawBone(drawingContext, JointType.HipCenter, JointType.HipRight);

                // Left Arm
                this.DrawBone(drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
                this.DrawBone(drawingContext, JointType.ElbowLeft, JointType.WristLeft);
                this.DrawBone(drawingContext, JointType.WristLeft, JointType.HandLeft);

                // Right Arm
                this.DrawBone(drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
                this.DrawBone(drawingContext, JointType.ElbowRight, JointType.WristRight);
                this.DrawBone(drawingContext, JointType.WristRight, JointType.HandRight);

                // Left Leg
                this.DrawBone(drawingContext, JointType.HipLeft, JointType.KneeLeft);
                this.DrawBone(drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
                this.DrawBone(drawingContext, JointType.AnkleLeft, JointType.FootLeft);

                // Right Leg
                this.DrawBone(drawingContext, JointType.HipRight, JointType.KneeRight);
                this.DrawBone(drawingContext, JointType.KneeRight, JointType.AnkleRight);
                this.DrawBone(drawingContext, JointType.AnkleRight, JointType.FootRight);
            }

            if (this.ShowJoints)
            {
                // Render Joints
                foreach (JointMapping joint in this.JointMappings.Values)
                {
                    Brush drawBrush = null;
                    switch (joint.Joint.TrackingState)
                    {
                        case JointTrackingState.Tracked:
                            drawBrush = this.trackedJointBrush;
                            break;
                        case JointTrackingState.Inferred:
                            drawBrush = this.inferredJointBrush;
                            break;
                    }

                    if (drawBrush != null)
                    {
                        drawingContext.DrawEllipse(drawBrush, null, joint.MappedPoint, JointThickness * this.ScaleFactor, JointThickness * this.ScaleFactor);
                    }
                }
            }
            double currentRightAngle = Convert.ToDouble(detect_wanted_right(this.Skeleton));
            double currentLeftAngle = Convert.ToDouble(detect_wanted_left(this.Skeleton));
            double currentKneeLeftAngle = Convert.ToDouble(detect_wanted_knee_left(this.Skeleton));
            double currentKneeRightAngle = Convert.ToDouble(detect_wanted_knee_right(this.Skeleton));
            double currentLegLeft = Convert.ToDouble(detect_wanted_leg_left(this.Skeleton));
            double currentLegRight = Convert.ToDouble(detect_wanted_leg_right(this.Skeleton));
            double currentslopeRight = Convert.ToDouble(slopeRight(this.Skeleton));
            double currentslopeLeft = Convert.ToDouble(slopeLeft(this.Skeleton));

            this.averageshoulderRightAngleTotal += currentRightAngle;
            this.averageshoulderLeftAngleTotal += currentLeftAngle;
            this.averageKneeLeftAngleTotal += currentKneeLeftAngle;
            this.averageKneeRightAngleTotal += currentKneeRightAngle;
            this.averageLegLeftAngleTotal += currentLegLeft;
            this.averageLegRightAngleTotal += currentLegRight;
            this.frameCount++;

            // 평균 계산
            if (this.frameCount % wantedFameCount == 0)  // 원하는 프레임 수에 도달하면 평균 계산
            {
                averageshoulderRightAngle = this.averageshoulderRightAngleTotal / this.frameCount;
                averageshoulderLeftAngle = this.averageshoulderLeftAngleTotal / this.frameCount;
                averageKneeLeftAngle = this.averageKneeLeftAngleTotal / this.frameCount;
                averageKneeRightAngle = this.averageKneeRightAngle / this.frameCount;
                averageLegLeftAngle = this.averageLegLeftAngleTotal / this.frameCount;
                averageLegRightAngle = this.averageLegRightAngleTotal / this.frameCount;
                // 여기에서 평균값을 파일이나 다른 위치에 저장하거나 사용할 수 있습니다
                // 초기화
                this.averageshoulderRightAngleTotal = 0.0;
                this.averageshoulderLeftAngleTotal = 0.0;
                this.averageKneeLeftAngleTotal = 0.0;
                this.averageKneeRightAngleTotal = 0.0;
                this.averageLegLeftAngleTotal = 0.0;
                this.averageLegRightAngleTotal = 0.0;

                this.frameCount = 0;
                averageshoulderRightAngle1 = averageshoulderRightAngle.ToString("F2");
                averageshoulderLeftAngle1 = averageshoulderLeftAngle.ToString("F2");
                averageKneeLeftAngle1 = averageKneeLeftAngle.ToString("F2");
                averageKneeRightAngle1 = averageKneeRightAngle.ToString("F2");
                averageLegLeftAngle1 = averageLegLeftAngle.ToString("F2");
                averageLegRightAngle1 = averageLegRightAngle.ToString("F2");

            }
            // 오른팔 왼팔 ,왼무릎,오른 무릎, 왼다리,오른다리
            fileWriter.WriteLine($"{"Average angle"}, {averageshoulderRightAngle1},{averageshoulderLeftAngle1},{averageKneeLeftAngle1},{averageKneeRightAngle1},{averageLegLeftAngle1},{averageLegRightAngle1}");
            allFileWriter.WriteLine($"{frameCount},{currentRightAngle},{currentLeftAngle},{currentKneeLeftAngle},{currentLegLeft},{currentLegRight}");
            //  pointFileWriter.WriteLine($"{this.Skeleton.Joints[JointType.AnkleRight].Position.X},{this.Skeleton.Joints[JointType.AnkleRight].Position.Y},{this.Skeleton.Joints[JointType.AnkleRight].Position.Z},{this.Skeleton.Joints[JointType.ShoulderRight].Position.X},{this.Skeleton.Joints[JointType.ShoulderRight].Position.Y},{this.Skeleton.Joints[JointType.ShoulderRight].Position.Z},{this.Skeleton.Joints[JointType.WristRight].Position.X},{this.Skeleton.Joints[JointType.WristRight].Position.Y},{this.Skeleton.Joints[JointType.WristRight].Position.Z}");
            pointFileWriter.WriteLine($"{currentslopeRight},{currentslopeLeft}");

            /*try
            {
                connection.Open();

                // 데이터 삽입 쿼리
                string query = "INSERT INTO users (username, email) VALUES ('wefasfef', '123@ananan.com')";

                // 쿼리 실행을 위한 MySqlCommand 객체 생성
                MySqlCommand command = new MySqlCommand(query, connection);

                // 쿼리 실행
                command.ExecuteNonQuery();

                Console.WriteLine("데이터 삽입 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine("데이터 삽입 중 오류 발생: " + ex.Message);
            }
            finally
            {
                // 연결 닫기
                connection.Close();
            }*/

        }

        private void DrawBone(DrawingContext drawingContext, JointType jointType1, JointType jointType2)
        {
            JointMapping joint1;
            JointMapping joint2;

            // If we can't find either of these joints, exit
            if (!this.JointMappings.TryGetValue(jointType1, out joint1) ||
                joint1.Joint.TrackingState == JointTrackingState.NotTracked ||
                !this.JointMappings.TryGetValue(jointType2, out joint2) ||
                joint2.Joint.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint1.Joint.TrackingState == JointTrackingState.Inferred &&
                joint2.Joint.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            drawPen.Thickness = InferredBoneThickness * this.ScaleFactor;
            if (joint1.Joint.TrackingState == JointTrackingState.Tracked && joint2.Joint.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
                drawPen.Thickness = TrackedBoneThickness * this.ScaleFactor;
            }

            drawingContext.DrawLine(drawPen, joint1.MappedPoint, joint2.MappedPoint);

        }
        private String slopeRight(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint ankleLeft = skeleton.Joints[JointType.AnkleRight];
                Joint hipLeft = skeleton.Joints[JointType.HipRight];
                double rldnfrl = Math.Abs(linear(ankleLeft.Position.X, ankleLeft.Position.Y, hipLeft.Position.X, hipLeft.Position.Y));

                return rldnfrl.ToString("F2");
            }
        }
        private String slopeLeft(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint ankleLeft = skeleton.Joints[JointType.AnkleLeft];
                Joint hipLeft = skeleton.Joints[JointType.HipLeft];
                double rldnfrl = Math.Abs(linear(ankleLeft.Position.X, ankleLeft.Position.Y, hipLeft.Position.X, hipLeft.Position.Y));

                return rldnfrl.ToString("F2");
            }
        }
        // 오른쪽 어깨 각도 측정
        private String detect_wanted_right(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint shoulderRightJoint = skeleton.Joints[JointType.ShoulderRight];
                Joint wristRightJoint = skeleton.Joints[JointType.WristRight]; // 오른손
                Joint ankleRight = skeleton.Joints[JointType.AnkleRight]; // 오른 발목
                String rightAngle = third(ankleRight.Position.X, ankleRight.Position.Y, ankleRight.Position.Z, shoulderRightJoint.Position.X, shoulderRightJoint.Position.Y, shoulderRightJoint.Position.Z, wristRightJoint.Position.X, wristRightJoint.Position.Y, wristRightJoint.Position.Z);

                return rightAngle;
            }
        }
        // 왼쪽 어깨 각도 측정
        private String detect_wanted_left(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft];
                Joint wristLeft = skeleton.Joints[JointType.WristLeft]; // 오른손
                Joint ankleLeft = skeleton.Joints[JointType.AnkleLeft]; // 오른 발목
                String rightAngle = third(ankleLeft.Position.X, ankleLeft.Position.Y, ankleLeft.Position.Z, shoulderLeft.Position.X, shoulderLeft.Position.Y, shoulderLeft.Position.Z, wristLeft.Position.X, wristLeft.Position.Y, wristLeft.Position.Z);

                return rightAngle;
            }
        }
        // 무릎 굽히고 90도 올리기
        private String detect_wanted_knee_left(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint hipLeft = skeleton.Joints[JointType.HipLeft];
                Joint kneeLeft = skeleton.Joints[JointType.KneeLeft]; // 오른손
                Joint ankleLeft = skeleton.Joints[JointType.AnkleLeft]; // 오른 발목
                String rightAngle = third(hipLeft.Position.X, hipLeft.Position.Y, hipLeft.Position.Z, kneeLeft.Position.X, kneeLeft.Position.Y, kneeLeft.Position.Z, ankleLeft.Position.X, ankleLeft.Position.Y, ankleLeft.Position.Z);

                return rightAngle;
            }
        }
        private String detect_wanted_knee_right(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint hipRight = skeleton.Joints[JointType.HipRight];
                Joint kneeRight = skeleton.Joints[JointType.KneeRight]; // 오른손
                Joint ankleRight = skeleton.Joints[JointType.AnkleRight]; // 오른 발목
                String rightAngle = third(hipRight.Position.X, hipRight.Position.Y, hipRight.Position.Z, kneeRight.Position.X, kneeRight.Position.Y, kneeRight.Position.Z, ankleRight.Position.X, ankleRight.Position.Y, ankleRight.Position.Z);

                return rightAngle;
            }
        }

        // 발 펴고 45도 올리기
        private String detect_wanted_leg_left(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint colume = skeleton.Joints[JointType.AnkleRight];
                Joint hipCenter = skeleton.Joints[JointType.HipCenter]; // 오른손
                Joint ankleLeft = skeleton.Joints[JointType.AnkleLeft]; // 오른 발목
                float realC = Math.Abs(colume.Position.X - hipCenter.Position.X);
                //float adsf = hipCenter.Position.Y + colume.Position.Y;

                String rightAngle = third(ankleLeft.Position.X, ankleLeft.Position.Y, ankleLeft.Position.Z, hipCenter.Position.X, hipCenter.Position.Y, hipCenter.Position.Z, colume.Position.X, realC, colume.Position.Z); ;

                return rightAngle;
            }
        }
        private String detect_wanted_leg_right(Skeleton skeleton)
        {
            if (skeleton == null) return "-inf";
            else
            {
                Joint colume = skeleton.Joints[JointType.AnkleLeft];
                Joint hipCenter = skeleton.Joints[JointType.HipCenter]; // 오른손
                Joint ankleRight = skeleton.Joints[JointType.AnkleRight]; // 오른 발목
                float realC = Math.Abs(colume.Position.X - hipCenter.Position.X);
                //float adsf = hipCenter.Position.Y + colume.Position.Y;

                String rightAngle = third(ankleRight.Position.X, ankleRight.Position.Y, ankleRight.Position.Z, hipCenter.Position.X, hipCenter.Position.Y, hipCenter.Position.Z, colume.Position.X, realC, colume.Position.Z); ;

                return rightAngle;
            }
        }
        static private double linear(float ankleX, float ankleY, float hipX, float hipY)
        {
            // y 좌표의 변화량 계산
            float deltaY = hipY - ankleY;

            // x 좌표의 변화량 계산
            float deltaX = hipX - ankleX;

            // 기울기 계산
            float slope = deltaY / deltaX;

            // 결과 반환
            return slope;
        }
        static private String distance()
        {
            return "";
        }
        static private String third(float ankleX, float ankleY, float ankleZ, float shoulderRightX, float shoulderRightY, float shoulderRightZ, float wristRightX, float wristRightY, float wristRightZ)
        {
            double[] vector1 = { ankleX - shoulderRightX, ankleY - shoulderRightY, ankleZ - shoulderRightZ };
            double[] vector2 = { wristRightX - shoulderRightX, wristRightY - shoulderRightY, wristRightZ - shoulderRightZ };

            double dotProduct = DotProduct(vector1, vector2);
            double magnitude1 = Magnitude(vector1);
            double magnitude2 = Magnitude(vector2);

            double cosTheta = dotProduct / (magnitude1 * magnitude2);
            double angleInRadians = Math.Acos(cosTheta);
            double angleInDegrees = RadiansToDegrees(angleInRadians);

            return angleInDegrees.ToString();
        }


        static private double DotProduct(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("벡터의 차원이 일치해야 합니다.");

            double result = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                result += vector1[i] * vector2[i];
            }

            return result;
        }

        static private double Magnitude(double[] vector)
        {
            double sumOfSquares = 0;
            foreach (var component in vector)
            {
                sumOfSquares += Math.Pow(component, 2);
            }

            return Math.Sqrt(sumOfSquares);
        }

        static private double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }



    }
}