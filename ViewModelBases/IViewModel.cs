using System;

namespace PrismWpf.Mvvm.ViewModelBases
{
    public interface IViewModel<out TModel> : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public TModel ModelObject { get; }
    }
}
