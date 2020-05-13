using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PrismWpf.Mvvm.ViewModelBases
{
    public class CollectionManagerViewModelBase<TModel, TSubM, TSubVM>
        : CollectionListenerViewModelBase<TModel, TSubM, TSubVM>
        where TModel : IEnumerable<TSubM>, ICollection<TSubM>, INotifyCollectionChanged
        where TSubVM : class
    {
        #region Fields
        private DelegateCommand<object> _addCommand;
        private DelegateCommand _deleteCommand;
        #endregion

        #region Properties
        public override TSubVM SelectedVMItem 
        { 
            get => base.SelectedVMItem; 
            set
            {
                if (SetProperty(ref _selectedVMItem, value))
                    DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand<object> AddCommand
            => _addCommand ?? (_addCommand = new DelegateCommand<object>(AddCommandExecute, CanAddCommandExecute));

        public DelegateCommand DeleteCommand
            => _deleteCommand ?? (_deleteCommand = new DelegateCommand(DeleteCommandExecute, CanDeleteCommandExecute));
        #endregion

        #region Constructor
        public CollectionManagerViewModelBase(TModel modelObject, IContainerExtension container)
            : base(modelObject, container)
        { }

        public CollectionManagerViewModelBase(IContainerExtension container)
            : base(container)
        { }
        #endregion

        #region Method
        protected virtual bool CanAddCommandExecute(object cmdParameter)
        {
            return true;
        }

        protected virtual void AddCommandExecute(object cmdParameter)
        {
            throw new NotImplementedException();
        }

        protected virtual bool CanDeleteCommandExecute()
        {
            return SelectedVMItem != null;
        }

        protected virtual void DeleteCommandExecute()
        {
            _dialogService.ShowDialog(
                "Confirmation Dialog",
                new DialogParameters("message=Please confirm the deletion."),
                r =>
                {
                    if (r.Result == ButtonResult.OK)
                        DefaultDeleteAction();
                });
        }

        protected virtual void DefaultDeleteAction()
        {
            if (_isTSubVMViewModelBaseSubType && ModelObject.Remove((SelectedVMItem as IViewModel<TSubM>).ModelObject))
            {
                SelectedVMItem = null;
            }
        }
        #endregion
    }
}
