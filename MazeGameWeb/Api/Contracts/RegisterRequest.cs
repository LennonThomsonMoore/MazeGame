namespace MazeGame.Api.Contracts
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public RegisterRequest(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }
    }
}
