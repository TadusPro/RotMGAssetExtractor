namespace RotMGAssetExtractor.ModelHelpers
{
    public class Projectile
    {
        public string ObjectId { get; set; }
        public int Speed { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int LifetimeMS { get; set; }
        public int Acceleration { get; set; }
        public int AccelerationDelay { get; set; }
        public int SpeedClamp { get; set; }
        public bool MultiHit { get; set; }
        public string Id { get; set; }
        public int Type { get; set; }
        public string Class { get; set; }
        public string Texture { get; set; }
        public float AngleCorrection { get; set; }
        public float Rotation { get; set; }
        public int Size { get; set; }
        public bool PassesCover { get; set; }
        public float Frequency { get; set; }
        public bool ArmorPiercing { get; set; }
        public float Amplitude { get; set; }
        public bool RandomTexture { get; set; }
        public string ConditionEffect { get; set; }
        public string Animation { get; set; }
        public bool FaceDir { get; set; }
        public bool ProtectFromSink { get; set; }
        public string ParticleTrail { get; set; }
        public float CollisionMult { get; set; }
        public bool Boomerang { get; set; }
        public bool Wavy { get; set; }
        public int Damage { get; set; }
        public bool Parametric { get; set; }
        public float DamageMultiplier { get; set; }
        public float CircleTurnDelay { get; set; }
        public float CircleTurnAngle { get; set; }
        public float TurnRate { get; set; }
        public float Magnitude { get; set; }
        public float TurnRateDelay { get; set; }
        public float TurnStopTime { get; set; }
        public int ShadowSize { get; set; }
        public string Labels { get; set; }
        public AnimatedTexture AnimatedTexture { get; set; }
        public float TurnAcceleration { get; set; }
        public float TurnAccelerationDelay { get; set; }
        public float TurnClamp { get; set; }
    }
}