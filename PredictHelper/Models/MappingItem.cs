using System;
using System.Collections.Generic;

namespace PredictHelper
{
    public class MappingItem : BaseInpc
    {
        private bool _IsActive;
        private ExistState _ExistState;

        public bool IsActive { get => _IsActive; set => SetField(ref _IsActive, value); }
        public ExistState ExistState { get => _ExistState; set => SetField(ref _ExistState, value); }
        public int ContentTypeId { get; set; }
        public Guid PredicateGuid { get; set; }

        public Dictionary<int, ContentType> ContentTypesDict;
        public string Name => ContentTypesDict[ContentTypeId].Name;

        public MappingItem()
        {
            ExistState = ExistState.New;
            this.PropertyChanged += MappingItem_PropertyChanged;
        }

        private void MappingItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExistState))
                return;

            switch (ExistState)
            {
                case ExistState.Initializing:
                    break;
                case ExistState.Default:
                    ExistState = ExistState.Updated;
                    break;
                case ExistState.New:
                    break;
                case ExistState.Updated:
                    break;
                case ExistState.ToBeDeleted:
                    ExistState = ExistState.Updated;
                    break;
                default:
                    throw new System.Exception("Неожиданная ситуация: не указан ExistState");
            }
        }
    }
}