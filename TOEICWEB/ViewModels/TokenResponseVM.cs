public class TokenResponseVM
{
    public string Token { get; set; } = string.Empty;
    public UserInfoVM User { get; set; } = new UserInfoVM();
}