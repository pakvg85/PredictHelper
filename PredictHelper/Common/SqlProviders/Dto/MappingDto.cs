using System;

namespace PredictHelper
{
    public class MappingDto
    {
        public int ContentTypeId { get; set; }
        public Guid PredicateGuid { get; set; }
        public bool IsActive { get; set; }
    }

    public class MappingDtoWithExistState : MappingDto
    {
        public ExistState ExistState { get; set; }
    }
}