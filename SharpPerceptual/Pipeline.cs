using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sense.Perceptual;

namespace SharpPerceptual {
    public class Pipeline : UtilMPipeline {
        private GestureSensor _gestures;
        private PoseSensor _poses;
        private Task _loopingTask;

        private const pxcmStatus NoError = pxcmStatus.PXCM_STATUS_NO_ERROR;
        public Hand LeftHand { get; private set; }
        public Hand RightHand { get; private set; }
        public Face Face { get; private set; }

        public IGestureSensor Gestures {
            get { return _gestures; }
        }

        public IPoseSensor Poses {
            get { return _poses; }
        }

        public Pipeline() {
            LeftHand = new Hand(Side.Left);
            RightHand = new Hand(Side.Right);
            Face = new Face();
            _gestures = new GestureSensor();
            _poses = new PoseSensor();
        }

        public void Start() {
            EnableGesture();
            EnableFaceLandmark();
            if (!Init()) {
                throw new Exception("Could not initialize the camera");
            }
            _loopingTask = Task.Run(() => {
                try {
                    Loop();
                }
                catch (Exception ex) {
                    Debug.WriteLine("EXCEPTION! " + ex);
                }
            });
        }

        private void Loop() {
            while (true) {
                AcquireFrame(true);
                TrackBodyPart(LeftHand, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT);
                TrackBodyPart(RightHand, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT);
                TrackFingers(LeftHand, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT);
                TrackFingers(RightHand, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT);

                var data = new PXCMFaceAnalysis.Detection.Data();
                var face = QueryFace();
                ulong timestamp;
                int faceId;
                face.QueryFace(0, out faceId, out timestamp);
                var location = (PXCMFaceAnalysis.Detection)face.DynamicCast(PXCMFaceAnalysis.Detection.CUID);
                location.QueryData(faceId, out data);
                location.QueryData(0, out data);
                Face.IsVisible = data.rectangle.x > 0;
                Face.Position = new Point3D {
                    X = (data.rectangle.x),
                    Y = (data.rectangle.y),
                };

                //Get face location
                //faceLocation = (PXCMFaceAnalysis.Detection)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Detection.CUID);
                //locationStatus = faceLocation.QueryData(faceId, out faceLocationData);
                //detectionConfidence = faceLocationData.confidence.ToString();
                //parent.label1.Text = "Confidence: " + detectionConfidence;

                ////Get face landmarks (eye, mouth, nose position, etc)
                //faceLandmark = (PXCMFaceAnalysis.Landmark)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Landmark.CUID);
                //faceLandmark.QueryProfile(1, out landmarkProfile);
                //faceLandmark.SetProfile(ref landmarkProfile);
                //faceLandmarkData = new PXCMFaceAnalysis.Landmark.LandmarkData[7];
                //landmarkStatus = faceLandmark.QueryLandmarkData(faceId, PXCMFaceAnalysis.Landmark.Label.LABEL_7POINTS, faceLandmarkData);

                ////Get face attributes (smile, age group, gender, eye blink, etc)
                //faceAttributes = (PXCMFaceAnalysis.Attribute)faceAnalysis.DynamicCast(PXCMFaceAnalysis.Attribute.CUID);
                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, 0, out attributeProfile);
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_EMOTION, faceId, out smile);

                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, 0, out attributeProfile);
                //attributeProfile.threshold = 50; //Must be here!
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_EYE_CLOSED, faceId, out blink);

                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, 0, out attributeProfile);
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_GENDER, faceId, out gender);

                //faceAttributes.QueryProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, 0, out attributeProfile);
                //faceAttributes.SetProfile(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, ref attributeProfile);
                //attributeStatus = faceAttributes.QueryData(PXCMFaceAnalysis.Attribute.Label.LABEL_AGE_GROUP, faceId, out age_group);

                
                ReleaseFrame();
                //Debug.WriteLine("L: {0}", LeftHand.GetInfo());
                //Debug.WriteLine("R: {0}", RightHand.GetInfo());
                //Debug.WriteLine("-----------------------------------------------");
            }
        }

        private void TrackBodyPart(BodyPart bodyPart, PXCMGesture.GeoNode.Label bodyLabel) {
            var values = new PXCMGesture.GeoNode();
            QueryGesture().QueryNodeData(0, bodyLabel, out values);
            SetBodyPartValues(bodyPart, values);
        }

        private void TrackFingers(Hand hand, PXCMGesture.GeoNode.Label targetHand) {
            TrackBodyPart(hand.Thumb, targetHand | PXCMGesture.GeoNode.Label.LABEL_FINGER_THUMB);
            TrackBodyPart(hand.Index, targetHand | PXCMGesture.GeoNode.Label.LABEL_FINGER_INDEX);
            TrackBodyPart(hand.Middle, targetHand | PXCMGesture.GeoNode.Label.LABEL_FINGER_MIDDLE);
            TrackBodyPart(hand.Ring, targetHand | PXCMGesture.GeoNode.Label.LABEL_FINGER_RING);
            TrackBodyPart(hand.Pinky, targetHand | PXCMGesture.GeoNode.Label.LABEL_FINGER_PINKY);
        }

        private static void SetBodyPartValues(BodyPart hand, PXCMGesture.GeoNode geoNode) {
            hand.IsVisible = geoNode.body > 0;
            if (!hand.IsVisible) {
                return;
            }
            hand.Position = new Point3D {
                X = geoNode.positionImage.x,
                Y = geoNode.positionImage.y,
                Z = geoNode.positionImage.z
            };
            if (geoNode.openness > 75) {
                hand.IsOpen = true;
            }
            else if (geoNode.openness < 10) {
                hand.IsOpen = false;
            }
        }

        public override void OnGesture(ref PXCMGesture.Gesture gesture) {
            base.OnGesture(ref gesture);
            Debug.WriteLine(gesture.body + " Gesture: {0} Visible: {1} Confidence: {2}", gesture.label, gesture.active, gesture.confidence);
            switch (gesture.label) {
                case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                    IfThisElseThat(gesture.active, _poses.OnBigFiveBegin, _poses.OnBigFiveEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
                    IfThisElseThat(gesture.active, _poses.OnPosePeaceBegin, _poses.OnPosePeaceEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN:
                    IfThisElseThat(gesture.active, _poses.OnPoseThumbsDownBegin, _poses.OnPoseThumbsDownEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                    IfThisElseThat(gesture.active, _poses.OnPoseThumbsUpBegin, _poses.OnPoseThumbsUpEnd);
                    break;
                case PXCMGesture.Gesture.Label.LABEL_HAND_CIRCLE:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_HAND_WAVE:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_DOWN:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_UP:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_RIGHT:
                    _gestures.OnGestureSwipeRight();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_NAV_SWIPE_LEFT:
                    _gestures.OnGestureSwipeLeft();
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_CUSTOMIZED:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_POSE:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_NAVIGATION:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_SET_HAND:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_MASK_DETAILS:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_MASK_SET:
                    break;
                case PXCMGesture.Gesture.Label.LABEL_ANY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void IfThisElseThat(bool value, Action ifTrue, Action ifFalse) {
            if (value) {
                ifTrue();
            }
            else {
                ifFalse();
            }
        }
    }
}