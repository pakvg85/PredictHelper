namespace PredictHelper
{
    public enum ExistState
    {
        /// <summary>
        /// Инициализация не закончена
        /// </summary>
        Initializing = -1,
        /// <summary>
        /// Нет изменений (считано из БД)
        /// </summary>
        Default = 0,
        /// <summary>
        /// Создано в текущей сессии
        /// </summary>
        New,
        /// <summary>
        /// Изменено в текущей сессии
        /// </summary>
        Updated,
        /// <summary>
        /// Помечено на удаление
        /// </summary>
        ToBeDeleted
    }
}