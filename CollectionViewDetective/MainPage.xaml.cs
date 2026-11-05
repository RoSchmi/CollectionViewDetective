using Android.Test.Suitebuilder.Annotation;
using AndroidX.RecyclerView.Widget;
using CollectionViewDetective.ViewModels;
using System.Diagnostics;
using static AndroidX.RecyclerView.Widget.RecyclerView;

namespace CollectionViewDetective
{
    public partial class MainPage : ContentPage
    {
        private readonly DisplayViewModel vm;
        int count = 0;

#if ANDROID
        private RecyclerView? _recyclerView;
      


      
#endif

        public MainPage(DisplayViewModel viewModel)
        {
            InitializeComponent();
            Loaded += DisplayPage_Loaded;
            vm = viewModel;
            BindingContext = vm;
            vm.UiDispatcher = this.Dispatcher;
        }



        private async void RecyclerView_LayoutChange(object? sender, Android.Views.View.LayoutChangeEventArgs e)
        {
            Debug.WriteLine($"\r\nRecyclerView_LayoutChanged at {DateTime.Now:HHH:mm:ss.fff}\r\n");
        }

        

    private async void DisplayPage_Loaded(object? sender, EventArgs e)
        {



#if ANDROID
            await Task.Delay(100);

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


                    _recyclerView?.SuppressLayout(suppress);

                };
            }

            Debug.WriteLine($"\r\n\r\n Printing androidhandler \r\n\r\n");

            Debug.WriteLine($"Type:      {type?.FullName}");
            Debug.WriteLine($"BaseType:  {type?.BaseType?.FullName}");
            Debug.WriteLine($"BaseType2: {type?.BaseType?.BaseType?.FullName}");

            Debug.WriteLine($"Layout Manager is:  { _recyclerView?.GetLayoutManager()?.GetType().FullName}");
            Debug.WriteLine($"\r\n\r\n Printed androidhandler \r\n\r\n");

            
#endif

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
