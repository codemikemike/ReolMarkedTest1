﻿// ViewModels/Base/ViewModelBase.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReolMarked.MVVM.ViewModels.Base
{
    /// <summary>
    /// Base class for all ViewModels
    /// Implementerer INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}