namespace RRWild.Models
{
    public sealed class WildPoint
    {
        public float x;
        public float y;
        public float z;
        public float yaw;

        public WildPoint() { }

        public WildPoint(float x, float y, float z, float yaw)
        {
            this.x = x; this.y = y; this.z = z; this.yaw = yaw;
        }
    }
}
