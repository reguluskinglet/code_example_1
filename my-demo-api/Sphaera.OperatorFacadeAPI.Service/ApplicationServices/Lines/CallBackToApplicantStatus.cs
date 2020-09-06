namespace demo.DemoApi.Service.ApplicationServices.Lines
{
    /// <summary>
    /// Статус операции начала звонка от пользователя к другому внешнему участнику
    /// </summary>
    public enum CallBackToApplicantStatus
    {
        /// <summary>
        /// Процесс звонка без ошибок
        /// </summary>
        Ok,

        /// <summary>
        /// Участник уже участвует в звонке по инциденту
        /// </summary>
        AlreadyInCall,
    }
}
