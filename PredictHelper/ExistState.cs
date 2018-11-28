namespace PredictHelper
{
    public enum ExistState
    {
        /// <summary>
        /// Нет изменений
        /// </summary>
        Default = 0,
        /// <summary>
        /// Создается
        /// </summary>
        New,
        /// <summary>
        /// Изменяется
        /// </summary>
        Updated,
        /// <summary>
        /// Помечен на удаление
        /// </summary>
        ToBeDeleted
    }
}