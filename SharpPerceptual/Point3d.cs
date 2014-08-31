namespace Sense.Perceptual {
    public struct Point3D {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public bool Equals(Point3D other) {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Point3D && Equals((Point3D)obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
    }
}