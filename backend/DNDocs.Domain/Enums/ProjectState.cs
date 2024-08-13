namespace DNDocs.Domain.Enums
{
    public enum ProjectState
    {
        /// <summary>
        /// builded and can be explored on docs.dndocs
        /// </summary>
        Active = 1,

        /// <summary>
        /// For some reason project is not active and cannot be browsed
        /// </summary>
        NotActive = 2
    }
}