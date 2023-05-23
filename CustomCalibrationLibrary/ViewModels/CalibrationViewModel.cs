using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using CustomCalibrationLibrary.Models;

namespace CustomCalibrationLibrary.ViewModels
{
    class CalibrationViewModel
    {
        protected CalibrationModel _model;

        private ObservableCollection<CalibrationPointViewModel> _calibrationPoints = new ObservableCollection<CalibrationPointViewModel>();
        public ObservableCollection<CalibrationPointViewModel> CalibrationPoints
        {
            get { return _calibrationPoints; }
        }

        public CalibrationViewModel(CalibrationModel model)
        {
            _model = model;
            foreach (CalibrationPoint item in _model.CalibrationPoints)
            {
                _calibrationPoints.Add(new CalibrationPointViewModel(item));
                item.PropertyChanged += OnCollectionItemChanged;
            }
            _model.CalibrationPoints.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (CalibrationPoint item in e.NewItems)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                _calibrationPoints.Add(new CalibrationPointViewModel(item.Position, item.Index));
                            });
                            item.PropertyChanged += OnCollectionItemChanged;
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems == null) break;
                        foreach (CalibrationPoint item in e.OldItems)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                _calibrationPoints.RemoveAt(item.Index);
                            });
                            item.PropertyChanged -= OnCollectionItemChanged;
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (CalibrationPoint item in e.NewItems)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                _calibrationPoints.Add(new CalibrationPointViewModel(item.Position, item.Index));
                            });
                            item.PropertyChanged += OnCollectionItemChanged;
                        }
                        if (e.OldItems == null) break;
                        foreach (CalibrationPoint item in e.OldItems)
                        {
                            item.PropertyChanged -= OnCollectionItemChanged;
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        Application.Current.Dispatcher.Invoke(() => {
                            _calibrationPoints.Move(e.OldStartingIndex, e.NewStartingIndex);
                        });
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        Application.Current.Dispatcher.Invoke(() => {
                            _calibrationPoints.Clear();
                        });
                        break;
                }
            }
        }
        private void OnCollectionItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            CalibrationPoint? typedSender = sender as CalibrationPoint;
            if (typedSender == null || e.PropertyName == null)
            {
                return;
            }
            CalibrationPointViewModel point = _calibrationPoints[typedSender.Index];
            PropertyInfo? piIn = typedSender.GetType().GetProperty(e.PropertyName);
            PropertyInfo? piOut = point.GetType().GetProperty(e.PropertyName);

            if (piIn == null || piOut == null)
            {
                return;
            }
            piOut.SetValue(point, piIn.GetValue(typedSender));
        }
    }
}
