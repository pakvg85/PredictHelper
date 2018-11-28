namespace PredictHelper
{
    public class PredicateDto
    {
        public int PredicateId { get; set; }
        public string Text { get; set; }
        public int GroupId { get; set; }
    }

    public class PredicateDtoWithExistState : PredicateDto
    {
        public ExistState ExistState { get; set; }
    }
}