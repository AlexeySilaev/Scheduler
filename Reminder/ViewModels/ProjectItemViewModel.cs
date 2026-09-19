using Reminder.Extensions;
using Reminder.Models;
using Reminder.Resources.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Reminder.ViewModels
{
    public class ProjectItemViewModel : SchedulerItemViewModel
    {
        private string _name;
        private DateTime _startDate;
        private DateTime? _endDate;
        private ObservableCollection<ProjectClause> _clauses;
        private ProjectClause _selectedClause;
        private EditingClauseMode _clauseMode;

        private ProjectItem ProjectItem
        {
            get { return (ProjectItem)_item; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (Name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                if (StartDate == value) return;
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                if (EndDate == value) return;
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public ObservableCollection<ProjectClause> Clauses
        {
            get { return _clauses; }
            set
            { 
                if(Clauses == value) return;
                _clauses = value;
                OnPropertyChanged(nameof(Clauses));
            }
        }

        public ProjectClause SelectedClause
        {
            get { return _selectedClause; }
            set
            {
                if (SelectedClause == value) return;
                if (CancelClause.CanExecute(null))
                    CancelClause.Execute(null);
                _selectedClause = value;
                OnPropertyChanged(nameof(SelectedClause));
                RefreshEditClauseButtons();
            }
        }

        public override ReminderItemType Type
        {
            get { return ReminderItemType.Project; }
        }
        
        public Command AddClause { get; }
        public Command EditClause { get; }
        public Command DeleteClause { get; }
        public Command SaveClause { get; }
        public Command CancelClause { get; }

        public ProjectItemViewModel(ProjectItem item) : base(item)
        {
            _name = item.Name;
            _startDate = item.StartDate;
            _endDate = item.EndDate;
            _clauseMode = EditingClauseMode.ViewItems;
            LoadClauses(item.Clauses);
            AddClause = new Command(OnAddClause, CanExecuteAddClause);
            EditClause = new Command(OnEditClause, CanExecuteEditClause);
            DeleteClause = new Command(OnDeleteClause, CanExecuteDeleteClause);
            SaveClause = new Command(OnSaveClause, CanExecuteSaveClause);
            CancelClause = new Command(OnCancelClause, CanExecuteCancelClause);
        }

        protected override bool CanExecuteEdit()
        {
            return true;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            ProjectItem.Name = Name;
            ProjectItem.StartDate = StartDate;
            ProjectItem.EndDate = EndDate;
            ProjectItem.Clauses = Clauses.Select(projectClause => projectClause.Text).ToArray();
            if (SaveClause.CanExecute(null))
            {
                SaveClause.Execute(null);
                SelectedClause = null;
            }
        }

        public override string Validate(bool isNew)
        {
            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(Name))
                result = "Необходимо заполнить название.";
            if (string.IsNullOrEmpty(result))
            {
                if (EndDate.HasValue && EndDate.Value < StartDate)
                    result = "Дата окончания меньше даты начала.";
            }
            return result;
        }

        public override void CancelEdit()
        {
            base.CancelEdit();
            Name = ProjectItem.Name;
            StartDate = ProjectItem.StartDate;
            EndDate = ProjectItem.EndDate;
            Clauses.Clear();
            LoadClauses(ProjectItem.Clauses);
        }

        private void LoadClauses(string[] clauses)
        { 
            Clauses = new ObservableCollection<ProjectClause>(clauses.Select(clause => new ProjectClause(clause)));
        }

        public override void Save()
        {
            ReminderModel.Instance.Projects.Add((ProjectItem)_item);
        }

        #region Функционал для пунктов проекта

        public EditingClauseMode ClauseMode
        {
            get { return _clauseMode; }
            private set
            {
                if (ClauseMode == value) return;
                _clauseMode = value;
                OnPropertyChanged(nameof(ClauseMode));
                RefreshEditClauseButtons();
            }
        }

        private void RefreshEditClauseButtons()
        {
            AddClause.RaiseCanExecuteChanged();
            EditClause.RaiseCanExecuteChanged();
            DeleteClause.RaiseCanExecuteChanged();
            SaveClause.RaiseCanExecuteChanged();
            CancelClause.RaiseCanExecuteChanged();
        }

        private bool CanExecuteAddClause()
        {
            return ClauseMode == EditingClauseMode.ViewItems;
        }

        private void OnAddClause()
        {
            ProjectClause clause = new ProjectClause(string.Empty);
            Clauses.Add(clause);
            SelectedClause = clause;
            ClauseMode = EditingClauseMode.EditNew;
        }

        private bool CanExecuteEditClause()
        {
            return SelectedClause != null && ClauseMode == EditingClauseMode.ViewItems;
        }

        private void OnEditClause()
        {
            ClauseMode = EditingClauseMode.EditExisting;
        }

        private bool CanExecuteDeleteClause()
        {
            return SelectedClause != null && ClauseMode == EditingClauseMode.ViewItems;
        }

        private void OnDeleteClause()
        {
            Clauses.Remove(SelectedClause);
            ClauseMode = EditingClauseMode.ViewItems;
        }

        private bool CanExecuteSaveClause()
        {
            return ClauseMode.In(EditingClauseMode.EditNew, EditingClauseMode.EditExisting);
        }

        private void OnSaveClause()
        {
            if (string.IsNullOrWhiteSpace(SelectedClause.BufferedText) == false)
            {
                ClauseMode = EditingClauseMode.ViewItems;
                SelectedClause.AcceptText();
            }
        }

        private bool CanExecuteCancelClause()
        {
            return ClauseMode.In(EditingClauseMode.EditNew, EditingClauseMode.EditExisting);
        }

        private void OnCancelClause()
        {
            if (ClauseMode == EditingClauseMode.EditNew)
                Clauses.Remove(SelectedClause);
            else
            { 
                Debug.Assert(ClauseMode == EditingClauseMode.EditExisting);
                SelectedClause.ResetText();
            }
            ClauseMode = EditingClauseMode.ViewItems;
        }


        public class ProjectClause : BaseViewModel
        {
            private string _text;
            private string _bufferedText;

            public string Text 
            {
                get { return _text; }
                private set 
                {
                    if (Text == value) return;
                    _text = value;
                    OnPropertyChanged(nameof(Text)); 
                }
            }

            /// <summary>
            /// Буфер для реализации отмены редактирования текста.
            /// Также убирает мелькание текста в списке пунктов при отмене редактирования.
            /// </summary>
            public string BufferedText
            {
                get { return _bufferedText; }
                set
                {
                    if (BufferedText == value) return;
                    _bufferedText = value;
                    OnPropertyChanged(nameof(BufferedText));
                }
            }


            public ProjectClause(string text)
            { 
                _text = text;
                _bufferedText = text;
            }

            public void AcceptText()
            {
                Text = BufferedText;
            }

            public void ResetText()
            {
                BufferedText = Text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        #endregion Функционал для пунктов проекта
    }

    public enum EditingClauseMode
    {
        ViewItems,
        EditNew,
        EditExisting
    }

}
