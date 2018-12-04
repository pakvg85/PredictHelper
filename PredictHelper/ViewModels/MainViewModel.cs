using NLog;
using PredictHelper.Common;
using PredictHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PredictHelper
{
    public class MainViewModel : BaseInpc
    {
        private GroupItem _CurrentGroup;
        private PredicateItem _CurrentPredicate;
        private string _StatusBarText;

        public Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        public Dictionary<int, ContentType> ContentTypesDict;
        public MainModel Model { get; set; }
        //public ObservableCollectionExt<GroupItem> GroupItems => Model.GroupItems;
        public GroupItem CurrentGroup
        {
            get => _CurrentGroup;
            set
            {
                SetField(ref _CurrentGroup, value);
                OnPropertyChanged(nameof(CurrentPredicates));
            }
        }
        public ObservableCollectionExt<PredicateItem> CurrentPredicates => CurrentGroup.PredicateItems;
        public PredicateItem CurrentPredicate
        {
            get => _CurrentPredicate;
            set
            {
                SetField(ref _CurrentPredicate, value);
                OnPropertyChanged(nameof(CurrentPredicateMappings));
            }
        }
        public ObservableCollectionExt<MappingItem> CurrentPredicateMappings => CurrentPredicate.MappingItems;
        public string StatusBarText { get => _StatusBarText; set => SetField(ref _StatusBarText, value); }

        private ICommand _command1;
        public ICommand Command1 => _command1 ?? (_command1 = new RelayCommand(o => Model.ProcessPredicates()));
        private ICommand _command2;
        public ICommand Command2 => _command2 ?? (_command2 = new RelayCommand(o => Model.DbSave()));
        private ICommand _command3;
        public ICommand Command3 => _command3 ?? (_command3 = new RelayCommand(o => OnDeleteButtonPressed(o)));
        private ICommand _command4;
        public ICommand Command4 => _command4 ?? (_command4 = new RelayCommand(o => ShowDialogAddPredicates()));
        private ICommand _command5;
        public ICommand Command5 => _command5 ?? (_command5 = new RelayCommand(o => Model.DbLoad(false)));

        public MainViewModel()
        {
            Model = new MainModel();

            Model.EventMessageOccured += model_EventMessageOccured;

            Model.DbLoad();
        }

        private void OnDeleteButtonPressed(object SelectedItems)
        {
            var selectedPredicatesViewModels = (SelectedItems as IList)?.OfType<PredicateItem>();
            if (selectedPredicatesViewModels == null || !selectedPredicatesViewModels.Any())
                return;

            try
            {
                var predicatesToBeTerminated = new List<PredicateItem>();
                foreach (var predicate in selectedPredicatesViewModels)
                {
                    if (predicate.ExistState == ExistState.New)
                        predicatesToBeTerminated.Add(predicate);
                    else
                        predicate.ExistState = ExistState.ToBeDeleted;

                    var mappingsToBeTerminated = new List<MappingItem>();
                    foreach (var mappingItem in predicate.MappingItems)
                    {
                        if (mappingItem.ExistState == ExistState.New)
                            mappingsToBeTerminated.Add(mappingItem);
                        else
                            mappingItem.ExistState = ExistState.ToBeDeleted;
                    }
                    predicate.MappingItems.RemoveRange(mappingsToBeTerminated);
                }

                CurrentPredicates.RemoveRange(predicatesToBeTerminated);
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
        }

        private void ShowDialogAddPredicates()
        {
            var dialog = new MessageBoxDialog.MessageBoxDialogWindow();
            if (dialog.ShowDialog() == true)
            {
                var newPredicateTexts = Regex.Split(dialog.ResponseText, Environment.NewLine);

                CurrentPredicates.AddRange(newPredicateTexts
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new PredicateItem { Text = x }));
            }
        }

        public void ProcessException(Exception ex, MessageImportance msgImp = MessageImportance.Error)
        {
            ProcessMessage(ex.Message + " (детальное инфо об ошибке - в файле лога в папке с программой)");

            if (msgImp == MessageImportance.Fatal)
                GlobalLogger.Fatal(ex, "UNHANDLED EXCEPTION");
            else
                GlobalLogger.Error(ex);
        }

        public void ProcessMessage(string msg, MessageImportance msgImp = MessageImportance.Info)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusBarText = msg;
            });
        }

        private void model_EventMessageOccured(object sender, MessageOccuredEventArgs e)
        {
            ProcessMessage(e.Message, e.MsgImportance);
        }
    }
}