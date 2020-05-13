using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PrismWpf.Mvvm.ViewModelBases
{
    public class CollectionListenerViewModelBase<TModel, TSubM, TSubVM>
        : CollectionViewModelBase<TModel, TSubM, TSubVM>
        where TModel : IEnumerable<TSubM>, INotifyCollectionChanged
        where TSubVM : class
    {
        #region Constructor
        public CollectionListenerViewModelBase(TModel modelObject, IContainerExtension container)
            : base(modelObject, container)
        { }

        public CollectionListenerViewModelBase(IContainerExtension container)
            : base(container)
        { }
        #endregion

        #region Method
        protected override void ModelEventsRegister()
        {
            base.ModelEventsRegister();
            _modelObject.CollectionChanged += ModelObject_CollectionChanged;
        }

        protected override void ModelEventsUnregister()
        {
            base.ModelEventsUnregister();
            _modelObject.CollectionChanged -= ModelObject_CollectionChanged;
        }

        private void ModelObject_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncOrAsyncInvoke(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            var opStartIndex = e.NewStartingIndex;
                            foreach (TSubM subModelItem in e.NewItems)
                            {
                                AddNewSubViewModel(subModelItem, opStartIndex++);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (TSubM subModelItem in e.OldItems)
                            {
                                RemoveSubViewModel(subModelItem);
                            }
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            ReplaceSubViewModel((TSubM)e.OldItems[0], (TSubM)e.NewItems[0], e.NewStartingIndex);
                            break;
                        }
                    case NotifyCollectionChangedAction.Move:
                        {
                            MoveSubViewModel(e.OldStartingIndex, e.NewStartingIndex);
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            ClearSubViewModel();
                            break;
                        }
                    default:
                        break;
                }
            });
        }

        protected virtual void RemoveSubViewModel(TSubM subModelItem)
        {
            if (_model2VMDic.ContainsKey(subModelItem))
            {
                var oldSubVMItem = _model2VMDic[subModelItem];
                SubViewModelEventsUnregister(oldSubVMItem);
                SubVMItemsCollection.Remove(oldSubVMItem);
                _model2VMDic.Remove(subModelItem);
                if (IsSubVMNeedDispose && oldSubVMItem is IDisposable disposableItem)
                    disposableItem.Dispose();
            }
            //else
            //    throw new Exception("Can't find the related sub ViewModel object.");
        }

        protected virtual void ReplaceSubViewModel(TSubM oldSubModelItem, TSubM newSubModelItem, int index)
        {

            if (_model2VMDic.ContainsKey(oldSubModelItem))
            {
                var oldSubVMItem = _model2VMDic[oldSubModelItem];
                SubViewModelEventsUnregister(oldSubVMItem);
                _model2VMDic.Remove(oldSubModelItem);
                if (IsSubVMNeedDispose && oldSubVMItem is IDisposable disposableItem)
                    disposableItem.Dispose();

                var newSubVMItem = GetSubViewModelInstance(newSubModelItem);
                SubViewModelEventsRegister(newSubVMItem);
                _model2VMDic[newSubModelItem] = newSubVMItem;
                SubVMItemsCollection.Add(newSubVMItem);
            }
            //else
            //    throw new Exception("The sub VM object related to oldSubModelItem dose not exist.");
        }

        protected virtual void MoveSubViewModel(int oldIndex, int newIndex)
        {
            SubVMItemsCollection.Move(oldIndex, newIndex);
        }

        protected virtual void ClearSubViewModel()
        {
            foreach (var subVMItem in SubVMItemsCollection)
            {
                SubViewModelEventsUnregister(subVMItem);
                if (IsSubVMNeedDispose && subVMItem is IDisposable disposableItem)
                    disposableItem.Dispose();
            }
            _model2VMDic.Clear();
            SubVMItemsCollection.Clear();
        }
        #endregion
    }
}
