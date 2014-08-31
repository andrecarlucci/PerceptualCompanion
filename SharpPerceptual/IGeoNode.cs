namespace Sense.Perceptual {
    public interface IGeoNode {
        PXCMGesture.GeoNode.Label BodyPart { get; }
        void UpdateState(PXCMGesture.GeoNode geoNode, PXCMGesture.Gesture gesture);
    }
}