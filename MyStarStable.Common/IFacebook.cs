namespace MyStarStable.Common
{
    public interface IFacebook
    {
        void Close(bool clearTokens);

        string UserId { get; set; }
        string UserEmail { get; set; }
    }
}
