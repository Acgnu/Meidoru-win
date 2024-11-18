using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AcgnuX.Controls
{
    class FadeAnimateItemsBehavior : Behavior<ItemsControl>
    {
        public DoubleAnimation Animation { get; set; }
        public TimeSpan Tick { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += new System.Windows.RoutedEventHandler(AssociatedObject_Loaded);
        }

        void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IEnumerable<UserControl> items;
            if (AssociatedObject.ItemsSource == null)
            {
                items = AssociatedObject.Items.Cast<UserControl>();
            }
            else
            {
                var itemsSource = AssociatedObject.ItemsSource;
                if (itemsSource is INotifyCollectionChanged)
                {
                    var collection = itemsSource as INotifyCollectionChanged;
                    collection.CollectionChanged += (s, cce) =>
                    {
                        if (cce.Action == NotifyCollectionChangedAction.Add)
                        {
                            var itemContainer = AssociatedObject.ItemContainerGenerator.ContainerFromItem(cce.NewItems[0]) as UserControl;
                            itemContainer.BeginAnimation(UserControl.OpacityProperty, Animation);
                        }
                    };

                }
                UserControl[] itemsSub = new UserControl[AssociatedObject.Items.Count];
                for (int i = 0; i < itemsSub.Length; i++)
                {
                    itemsSub[i] = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as UserControl;
                }
                items = itemsSub;
            }
            foreach (var item in items)
            {
                item.Opacity = 0;
            }
            var enumerator = items.GetEnumerator();
            if (enumerator.MoveNext())
            {
                DispatcherTimer timer = new DispatcherTimer() { Interval = Tick };
                timer.Tick += (s, timerE) =>
                {
                    var item = enumerator.Current;
                    item.BeginAnimation(UserControl.OpacityProperty, Animation);
                    if (!enumerator.MoveNext())
                    {
                        timer.Stop();
                    }
                };
                timer.Start();
            }
        }
    }
}
