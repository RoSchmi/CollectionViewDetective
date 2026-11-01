using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace CollectionViewDetective.Extensions
{
    public class ObservableCollectionExtended<T> : ObservableCollection<T>
    {
        private bool _suspend;

        public void SuspendNotifications() => _suspend = true;

        public void ResumeNotifications(bool sendReset = true)
        {
            _suspend = false;
            if (sendReset)
            {
                base.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
            }
            /*
            else
            {
                base.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace));
            }
            */
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suspend)
            {
                base.OnCollectionChanged(e);
            }
        }

        public void ReplaceAll(IEnumerable<T> items)
        {
            SuspendNotifications();
            base.ClearItems();
            foreach (var item in items)
                base.Add(item);
            ResumeNotifications(sendReset: true);
        }
    }
}
