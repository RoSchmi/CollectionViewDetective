using CollectionViewDetective.ViewModels;
using System.Diagnostics;

namespace CollectionViewDetective
{
    public partial class MainPage : ContentPage
    {
        private readonly DisplayViewModel vm;
        int count = 0;

        public MainPage(DisplayViewModel viewModel)
        {
            InitializeComponent();
            vm = viewModel;
            BindingContext = vm;
            vm.UiDispatcher = this.Dispatcher;
        }

        private void OnVerticallyScrolledCollection(object sender, ItemsViewScrolledEventArgs e)
        {
            if (BindingContext is DisplayViewModel vm)
            {
                vm.ActualTopVisibleItemIndex = e.FirstVisibleItemIndex;
                vm.ActualBottomVisibleItemIndex = e.LastVisibleItemIndex;

                Debug.WriteLine($"VerticallyScrolled {DateTime.Now:HH:mm:ss.fff}  FirstVisibleRow: {e.FirstVisibleItemIndex}");
                vm.VerticallyScrolledCollection(e);
            }
        }

        private void OnHorizontallyScrolledTable(object sender, ScrolledEventArgs e)
        {
            /*
            if (_syncing)
                return;
            
            _syncing = true;
            ScrollHeader.ScrollToAsync(e.ScrollX, 0, false);
            _syncing = false;
            */
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext is DisplayViewModel vm)
            {
                vm.RequestScrollToIndex = (targetIndex) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Debug.WriteLine($"In Code-Behind. ScrollToIndex {targetIndex}");
                        DisplayCollectionView.ScrollTo(
                            targetIndex,
                            position: ScrollToPosition.Start,
                            animate: false);
                    });
                };
                vm.RequestScrollToItem = (targetItem) => {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Debug.WriteLine($"In Code-Behind. ScrollToItem {targetItem}");
                        DisplayCollectionView.ScrollTo(
                            targetItem,
                            position: ScrollToPosition.Start,
                            animate: false);
                    });

                };
            }
        }  
    }
}
