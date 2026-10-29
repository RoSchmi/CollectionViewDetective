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
    }
}
