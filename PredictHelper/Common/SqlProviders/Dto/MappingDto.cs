namespace PredictHelper
{
    public class MappingDto
    {
        public int PredicateId { get; set; }
        public int ContentTypeId { get; set; }
        public bool IsActive { get; set; }
    }

    public class MappingDtoWithExistState: MappingDto
    {
        public ExistState ExistState { get; set; }
    }
}