namespace MazeGame.Api.Contracts
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginRequest(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }
    }
}
