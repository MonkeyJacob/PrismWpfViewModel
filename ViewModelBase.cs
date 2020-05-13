using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace PrismWpf.Mvvm.ViewModelBases
{
    public class ViewModelBase<TModel> :  BindableBase, IViewModel<TModel>
    {
        #region Fields
        protected readonly IContainerExtension _container;
        protected readonly IEventAggregator _eventAgg;
        protected readonly IDialogService _dialogService;
        protected TModel _modelObject;
        protected bool _disposed = false;
        #endregion

        #region Properties
        public TModel ModelObject
        {
            get
                => _modelObject;
            //{
            //    return
            //        _modelObject != null ?
            //        _modelObject :
            //        throw new NullReferenceException($"The ModelObject of {GetType().Name} is null.");
            //}
            set
            {
                if (value != null)
                {
                    if (!Equals(_modelObject, value))
                    {
                        if (_modelObject != null)
                            ModelEventsUnregister();

                        SetProperty(ref _modelObject, value);
                        SyncOrAsyncInvoke(InitializeFromModel);
                    }
                }
                else
                    //_dialogService.ShowDialog(
                    //    "Warning",
                    //    new DialogParameters(
                    //        $"message={$"The value of the ModelObject property of {GetType().Name} view model is not allowed to be set to null."}"),
                    //    r => { },
                    //    "NotificationDialog");
                    throw new Exception($"The value of the ModelObject property of {GetType().Name} view model is not allowed to be set to null.");
            }
        }
        #endregion

        #region Constructors
        public ViewModelBase(TModel modelObject, IContainerExtension container)
            : this(container)
        {
            ModelObject = modelObject;
        }

        public ViewModelBase(IContainerExtension container)
        {
            _container = container;
            _eventAgg = container.Resolve<IEventAggregator>();
            _dialogService = container.Resolve<IDialogService>();
            AggregatorEventsRegister();
        }
        #endregion

        #region Protected Methods
        protected virtual void AggregatorEventsRegister()
        { }

        protected virtual void AggregatorEventsUnregister()
        { }

        protected virtual void InitializeFromModel()
        {
            ModelEventsRegister();
        }

        protected virtual void ModelEventsRegister()
        {
            if (_modelObject is INotifyPropertyChanged notifyObj)
                notifyObj.PropertyChanged += ModelObject_PropertyChanged;
        }

        protected virtual void ModelEventsUnregister()
        {
            if (_modelObject is INotifyPropertyChanged notifyObj)
                notifyObj.PropertyChanged -= ModelObject_PropertyChanged;
        }

        protected virtual void ModelObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        { }

        protected virtual void SyncOrAsyncInvoke(Action specficAction)
        {
            if (!Thread.CurrentThread.ManagedThreadId.Equals(MainThreadRecordHelper.MaintThreadID))
                Application.Current.Dispatcher.Invoke(specficAction);
            else
                specficAction();
        }
        #endregion

        #region Set Model Property Methods
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TModelProp"></typeparam>
        /// <param name="value"></param>
        /// <param name="isAllowEmptyStr"></param>
        /// <param name="modelPropName"></param>
        /// <param name="setPropertyAction"></param>
        /// <param name="convertFunc"></param>
        /// <param name="callerPropName"></param>
        protected void InteractiveSetModelProperty<TValue, TModelProp>(
            TValue value
            , bool isAllowEmptyStr = false
            , [CallerMemberName] string modelPropName = ""
            , Action<object, string> setPropertyAction = null
            , Func<TValue, TModelProp> convertFunc = null
            , [CallerMemberName] string callerPropName = "")
        {
            try
            {
                var valueType = typeof(TValue);

                if (valueType == typeof(string) && !isAllowEmptyStr && string.IsNullOrWhiteSpace(value.ToString()))
                    throw new Exception("Empty or whitspace string is not allowed");

                var modelPropType = typeof(TModelProp);
                var setAction = setPropertyAction ?? SetModelProperty;
                if (modelPropType == valueType)
                    setAction((TModelProp)Convert.ChangeType(value, modelPropType), modelPropName);
                else
                {
                    if (convertFunc != null)
                        setAction(convertFunc(value), modelPropName);
                    else
                        throw new Exception("Type of viewmodel property is not type of model property. Convert function can't be null");
                }

            }
            catch (Exception ex)
            {
                var exMsg = InvokeExceptionUnpack(ex);
                _dialogService.ShowDialog(
                    "Warning",
                    new DialogParameters($"message={exMsg}"),
                    r => { },
                    "NotificationDialog");
            }
            finally
            {
                RaisePropertyChanged(callerPropName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="modelpropName"></param>
        protected void SetModelProperty(object value, string modelpropName)
        {
            var (ownerObj, propInfo) = GetModelPropertyInfo(modelpropName);

            if (!Equals(propInfo.GetValue(ownerObj), value))
                propInfo.SetValue(ownerObj, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propPathName"></param>
        /// <returns></returns>
        protected (object ownerObj, PropertyInfo propInfo) GetModelPropertyInfo(string propPathName)
        {
            var propNamesArr = propPathName.Split('.');
            PropertyInfo propInfo = _modelObject.GetType().GetProperty(propNamesArr[0]);
            object propObj = propInfo.GetValue(_modelObject);

            for (int index = 1; index < propNamesArr.Length && propInfo != null; index++)
            {
                propInfo = propObj.GetType().GetProperty(propNamesArr[index]);
                if (propInfo != null)
                {
                    if (index < propNamesArr.Length - 1)
                        propObj = propInfo.GetValue(propObj);
                }
                else
                    throw new Exception($"{ propInfo.GetType().Name } does not contain a definition for '{propNamesArr[index]}.");
            }

            return (propObj, propInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected string InvokeExceptionUnpack(Exception ex)
        {
            while (ex is TargetInvocationException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }
        #endregion

        #region IDisposable Interface Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                AggregatorEventsUnregister();
                ModelEventsRegister();
            }

            _disposed = true;
        }
        #endregion
    }
}
