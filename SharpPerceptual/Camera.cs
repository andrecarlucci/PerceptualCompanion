using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sense.Perceptual {
    public class Camera : IDisposable {
        public const int ResolutionWidth = 320;
        public const int ResolutionHeight = 240;
        public static int CameraScanInMilli = 200;

        private bool _active;
        private UtilMPipeline _pipeline;
        private List<IGeoNode> _monitoringObjects;
        private Task _loopingTask;

        public Camera() {
            _pipeline = new UtilMPipeline();
            _monitoringObjects = new List<IGeoNode>();
        }

        public void Monitor(IGeoNode geoNode) {
            _monitoringObjects.Add(geoNode);
        }

        public void Start() {
            _pipeline.EnableGesture();            
            _pipeline.EnableFaceLandmark();
            if (!_pipeline.Init()) {
                throw new Exception("Could not initialize the camera");
            }
            _active = true;
            _loopingTask = Task.Factory.StartNew(CaptureEvents);
        }

        private async void CaptureEvents() {
            while (_active) {
                _pipeline.AcquireFrame(true);
                _monitoringObjects.ForEach(ScanAndUpdate);
                _pipeline.ReleaseFrame();
                //await TaskEx.Delay(CameraScanInMilli);
            }
        }
        //poses (parado) Two notifications: start/end
        //gestures set o changing poses over time. Notification: end of gesture
        //alert: Label -> tracking part out-of-view
        private void ScanAndUpdate(IGeoNode node) {
            //var geoNode = new PXCMGesture.GeoNode();
            //var gesture = new PXCMGesture.Gesture();
            //int fid;
            //ulong timestamp;
            //_pipeline.QueryGesture().QueryGestureData(0, node.BodyPart, 0, out gesture);
            //_pipeline.QueryGesture().QueryNodeData(0, node.BodyPart, out geoNode);

            //var face = _pipeline.QueryFace();
            //PXCMFaceAnalysis.Detection.Data data;
            //var location = face.DynamicCast<PXCMFaceAnalysis.Detection>(0);
            //location.QueryData(0, out data);
            ////data.rectangle;
            
            //var landmark = face.DynamicCast<PXCMFaceAnalysis.Landmark>(0);
            //landmark.QueryLandmarkData(0, PXCMFaceAnalysis.Landmark.Label.LABEL_NOSE_TIP, )
            //node.UpdateState(geoNode, gesture);
        }

        

        public void Dispose() {
            _active = false;
            _loopingTask.Wait(TimeSpan.FromSeconds(5));
            _pipeline.Dispose();
        }
    }
}