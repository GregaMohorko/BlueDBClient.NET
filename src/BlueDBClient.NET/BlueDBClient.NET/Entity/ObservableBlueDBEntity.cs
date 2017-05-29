using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDBClient.NET.Entity
{
	/// <summary>
	/// A base class for entities that must implement INotifyPropertyChanged interface.
	/// <para>
	/// Use RaisePropertyChanged and Set methods.
	/// </para>
	/// </summary>
	public abstract class ObservableBlueDBEntity : BlueDBEntity, INotifyPropertyChanged, INotifyPropertyChanging
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>
		/// Raises the PropertyChanged event. You do not need to call this manually if you use the Set method.
		/// </summary>
		/// <param name="propertyName">The name of the property that changed.</param>
		public virtual void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises the PropertyChanging event. You do not need to call this manually if you use the Set method.
		/// </summary>
		/// <param name="propertyName">The name of the property that is about to be changed.</param>
		public virtual void RaisePropertyChanging(string propertyName)
		{
			PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
		}

		/// <summary>
		/// Compares the current and new value of the specified field. If it differs, then it raises PropertyChanging event, assigns the new value and raises the PropertyChanged event.
		/// </summary>
		/// <typeparam name="T">The type of the property that is being set.</typeparam>
		/// <param name="propertyName">The name of the property that is being set.</param>
		/// <param name="field">The field that is being set.</param>
		/// <param name="newValue">The new field value that is about to be set.</param>
		protected bool Set<T>(string propertyName,ref T field,T newValue)
		{
			if(EqualityComparer<T>.Default.Equals(field, newValue)) {
				// are the same
				return false;
			}

			RaisePropertyChanging(propertyName);
			field = newValue;
			RaisePropertyChanged(propertyName);
			return true;
		}
	}
}
