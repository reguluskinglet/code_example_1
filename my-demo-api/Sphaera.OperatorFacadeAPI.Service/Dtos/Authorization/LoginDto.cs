namespace demo.DemoApi.Service.Dtos.Authorization
{
    /// <summary>
    /// Dto, содержащая данные для входа в систему
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Логин
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }
    }
}
