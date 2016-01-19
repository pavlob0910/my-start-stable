namespace MyStarStable.Common
{
    public interface IHMACSHA256
    {
        byte[] ComputeHash(byte[] secret, byte[] message);
    }
}
