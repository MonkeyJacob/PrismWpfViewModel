using Prism.Ioc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PrismWpf.Mvvm.ViewModelBases
{
    public class CollectionViewModelBase<TModel, TSubM, TSubVM>
        : ViewModelBase<TModel>, IEnumerable<TSubVM>, INotifyCollectionChanged
        where TModel : IEnumerable<TSubM>
        where TSubVM : class
    {
        #region Fields
        protected Dictionary<TSubM, TSubVM> _model2VMDic;
        protected TSubVM _selectedVMItem;
        protected bool _isTSubVMViewModelBaseSubType;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region Properties
        public ObservableCollection<TSubVM> SubVMItemsCollection { get; protected set; }

        public TSubVM this[int index]
        {
            get
            {
                if (SubVMItemsCollection != null
                    && SubVMItemsCollection.Count > 0
                    && index >= 0
                    && index < SubVMItemsCollection.Count)
                    return SubVMItemsCollection[index];
                else
                    return null;
            }
            set
            {
                if (SubVMItemsCollection != null
                   && SubVMItemsCollection.Count > 0
                   && index >= 0
                   && index < SubVMItemsCollection.Count)
                    SubVMItemsCollection[index] = value;
            }
        }

        public virtual TSubVM SelectedVMItem
        {
            get => _selectedVMItem;
            set => SetProperty(ref _selectedVMItem, value);
        }

        public bool IsSubVMNeedDispose { get; set; } = true;
        #endregion

        #region Constructor
        public CollectionViewModelBase(TModel modelObject, IContainerExtension container)
            : base(modelObject, container)
        { }

        public CollectionViewModelBase(IContainerExtension container)
             : base(container)
        { }
        #endregion

        #region Methods
        
        #region Public Method
        public virtual TSubVM FindVMByModel(TSubM subModelItem)
        {
            if (subModelItem != null && _model2VMDic.ContainsKey(subModelItem))
                return _model2VMDic[subModelItem];
            else
                return null;
        }
        #endregion

        #region Override ViewModelBase Method
        protected override void InitializeFromModel()
        {
            _isTSubVMViewModelBaseSubType = CheckViewModelBaseSubType();
            InitializeData();
            ModelEventsRegister();
        }

        protected virtual void InitializeData()
        {
            if (_model2VMDic == null)
                _model2VMDic = new Dictionary<TSubM, TSubVM>();
            else
                _model2VMDic.Clear();

            if (SubVMItemsCollection == null)
            {
                SubVMItemsCollection = new ObservableCollection<TSubVM>();
                SubVMItemsCollection.CollectionChanged += (sender, e) => CollectionChanged?.Invoke(this, e);
            }
            else
            {
                foreach (var subVMItem in SubVMItemsCollection)
                {
                    SubViewModelEventsUnregister(subVMItem);
                    if (IsSubVMNeedDispose && subVMItem is IDisposable disposableItem)
                        disposableItem.Dispose();
                }
                SubVMItemsCollection.Clear();
            }

            foreach (var subModelItem in ModelObject)
            {
                AddNewSubViewModel(subModelItem);
            }
        }

        protected virtual void AddNewSubViewModel(TSubM newSubModelItem, int index = -1)
        {
            if (index == -1)
                index = SubVMItemsCollection.Count;
            var newSubVM = GetSubViewModelInstance(newSubModelItem);
            SubVMItemsCollection.Insert(index, newSubVM);
            _model2VMDic[newSubModelItem] = newSubVM;
            SubViewModelEventsRegister(newSubVM);
        }

        protected virtual TSubVM GetSubViewModelInstance(TSubM subModelItem)
        {
            if (_isTSubVMViewModelBaseSubType)
                return (TSubVM)Activator.CreateInstance(typeof(TSubVM), new object[] { subModelItem, _container });
            else
                throw new Exception($"TSubVM:{typeof(TSubVM)} is not a sub_type of ViewModelBase, should overide GetSubViewModelInstance method.");
        }

        protected virtual void SubViewModelEventsRegister(TSubVM subVMItem)
        { }

        protected virtual void SubViewModelEventsUnregister(TSubVM subVMItem)
        { }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                AggregatorEventsUnregister();
                ModelEventsRegister();
                foreach (var subVMItem in SubVMItemsCollection)
                {
                    SubViewModelEventsUnregister(subVMItem);
                    if (IsSubVMNeedDispose && subVMItem is IDisposable disposableItem)
                        disposableItem.Dispose();
                }
            }
        }

        private bool CheckViewModelBaseSubType()
        {
            return
                typeof(TSubVM) == typeof(ViewModelBase<TSubM>) ||
                typeof(TSubVM).IsSubclassOf(typeof(ViewModelBase<TSubM>));
        }
        #endregion

        #region IEnumerable Interface Implement
        public IEnumerator<TSubVM> GetEnumerator()
        {
            return SubVMItemsCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SubVMItemsCollection.GetEnumerator();
        }
        #endregion
    }
}
