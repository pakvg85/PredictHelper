using System.Collections.Generic;

namespace PredictHelper
{
    public class MappingItemViewModel : ViewModelBase
    {
        private int _ContentTypeId;
        private bool _IsActive;
        private ExistState _ExistState;

        public int ContentTypeId
        {
            get { return _ContentTypeId; }
            set
            {
                if (_ContentTypeId == value)
                    return;
                _ContentTypeId = value;
                OnPropertyChanged();
            }
        }
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive == value)
                    return;
                _IsActive = value;
                OnPropertyChanged();
            }
        }
        public ExistState ExistState
        {
            get { return _ExistState; }
            set
            {
                if (_ExistState == value)
                    return;
                _ExistState = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(ExistStateText));
            }
        }

        public Dictionary<int, ContentType> ContentTypesDict;
        public string Name => ContentTypesDict[ContentTypeId].Name;
        public string ExistStateText => ExistState == ExistState.Default ? " "
                                      : ExistState == ExistState.New ? "New"
                                      : ExistState == ExistState.Updated ? "Upd"
                                      : ExistState == ExistState.ToBeDeleted ? "Del"
                                      : throw new System.Exception("Invalid ExisState value");

        public MappingItemViewModel()
        {
            ExistState = ExistState.Initializing;
            this.PropertyChanged += MappingItemViewModel_PropertyChanged;
        }

        private void MappingItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExistState) || e.PropertyName == nameof(ExistStateText))
                return;

            switch (ExistState)
            {
                case ExistState.Initializing:
                    break;
                case ExistState.Default:
                    ExistState = ExistState.Updated;
                    //ExistStateChanged?.Invoke(this, new System.EventArgs());
                    break;
                case ExistState.New:
                    break;
                case ExistState.Updated:
                    break;
                case ExistState.ToBeDeleted:
                    ExistState = ExistState.Updated;
                    //ExistStateChanged?.Invoke(this, new System.EventArgs());
                    break;
                default:
                    throw new System.Exception("Неожиданная ситуация: не указан ExistState");
            }
        }
    }
}