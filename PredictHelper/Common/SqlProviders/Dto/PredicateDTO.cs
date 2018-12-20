using System;

namespace PredictHelper
{
    public class PredicateDto
    {
        public Guid Guid { get; set; }
        public Guid GroupGuid { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public byte? SideGroupId { get; set; }
        //public int? AdviceGroupId { get; set; }
    }

    public class PredicateDtoWithExistState : PredicateDto
    {
        public ExistState ExistState { get; set; }
    }
}