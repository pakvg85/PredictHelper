namespace PredictHelper
{
    public class PredicateMappingDto
    {
        public int PredicateId { get; set; }
        public int ContentTypeId { get; set; }
        public bool IsActive { get; set; }
    }

    public class PredicateMappingDtoWithExistState: PredicateMappingDto
    {
        public ExistState ExistState { get; set; }
    }
}