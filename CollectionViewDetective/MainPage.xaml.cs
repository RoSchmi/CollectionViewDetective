#if ANDROID
using AndroidX.RecyclerView.Widget;
using static AndroidX.RecyclerView.Widget.RecyclerView;
#endif
using CollectionViewDetective.ViewModels;
using System.Diagnostics;


namespace CollectionViewDetective
{
    public partial class MainPage : ContentPage
    {
        private readonly DisplayViewModel vm;
        
    #if ANDROID
        private RecyclerView? _recyclerView;
#endif

#region region Constructor
        public MainPage(DisplayViewModel viewModel)
        {
            InitializeComponent();  
            vm = viewModel;
            BindingContext = vm;
            DisplayCollectionView.HandlerChanged += DisplayCollectionView_HandlerChanged;
            vm.UiDispatcher = this.Dispatcher;
        }
#endregion



#if ANDROID
        private async void RecyclerView_LayoutChange(object? sender, Android.Views.View.LayoutChangeEventArgs e)
        {
            if (_recyclerView != null)
            {
                Debug.WriteLine($"\r\nRecyclerView_LayoutChanged at {DateTime.Now:HHH:mm:ss.fff}\r\n");
                Debug.WriteLine($"ChildCount = {_recyclerView.ChildCount}");
            }
        }

#endif
        private void DisplayCollectionView_HandlerChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine($"Handler Changed at {DateTime.Now:HH:mm:ss.fff}");
            if (BindingContext is DisplayViewModel vm)
            {
#if ANDROID

                var type = DisplayCollectionView.Handler?.PlatformView?.GetType();

                _recyclerView = DisplayCollectionView.Handler?.PlatformView as RecyclerView;

                if (_recyclerView != null)
                {
                    _recyclerView.HasFixedSize = true;       // important. Prevents stuttering vertical scrolling

                    _recyclerView.LayoutChange += RecyclerView_LayoutChange;

                    _recyclerView.ViewTreeObserver.GlobalLayout += (_, _) =>
                    {
                        Debug.WriteLine($"GlobalLayout {DateTime.Now:HH:mm:ss.fff}");
                    };
                    vm.RequestSuppressLayout = suppress =>
                    {
                        Debug.WriteLine($"Suppressed Layout ({suppress}) {DateTime.Now:HH:mm:ss.fff}");
                        _recyclerView?.SuppressLayout(suppress);

                    };

                    // vm.RequestSuppressLayout?.Invoke(true);
                }
#endif
            }
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
