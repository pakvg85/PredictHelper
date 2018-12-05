using System;

namespace PredictHelper
{
    public class GroupDto
    {
        public Guid Guid { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
    }

    public class GroupDtoWithExistState : GroupDto
    {
        public ExistState ExistState { get; set; }
    }
}