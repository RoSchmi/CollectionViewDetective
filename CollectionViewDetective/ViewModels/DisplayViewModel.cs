using CollectionViewDetective.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace CollectionViewDetective.ViewModels
{
    public partial class DisplayViewModel : ObservableObject
    {
        public Action<int>? RequestScrollToIndex { get; set; }

        public Action<EntityDisplayItem>? RequestScrollToItem { get; set; }

        private static readonly Stopwatch SW = Stopwatch.StartNew();

        private const int UiCollectionSizeInRows = 100;      // Größe der DisplayItemCollection
       // public int UiWindowStartIndex { get; private set; } = 0;
        public int UiWindowEndIndex => UiWindowStartIndex + UiCollectionSizeInRows - 1;

        [ObservableProperty]
        private int uiWindowStartIndex;

        [ObservableProperty]
        private int actualTopVisibleItemIndex;

        [ObservableProperty]
        private int actualBottomVisibleItemIndex;
        
        List<EntityDisplayItem> _AllItemsList = new List<EntityDisplayItem>();

        public ObservableCollection<EntityDisplayItem> DisplayItemCollection { get; set; } = new ObservableCollection<EntityDisplayItem>();
            
      //  public List<EntityDisplayItem> DisplayItemList { get; set; } = new List<EntityDisplayItem>();

        [ObservableProperty]
        private string buttonText = "Re-Initialize Tables";

        public IDispatcher UiDispatcher { get; set; }

        private CancellationTokenSource? _shiftDelayCts;

        private bool _isShiftingDown = false;

        public int UiScrollThreshold { get; } = 20;    // Schwelle für Fenster-Verschiebung

        #region Region Constructor
        public DisplayViewModel()
        {
            InitializeAllItems(2000);
            InitializeDisplayItemCollection(UiCollectionSizeInRows);
            DisplayItemCollection.CollectionChanged += OnDisplayCollectionChanged;
        }
        #endregion

        #region  RelayCommand GetNextData
        [RelayCommand]
        private Task GetNextData()
        {
            Debug.WriteLine($"Entering GetNextData");
            if (_isShiftingDown)
            {
                Debug.WriteLine($"Leaving GetNextData, shifting already on progress)");
                return Task.CompletedTask;
            }
            else
            {
                _isShiftingDown = true;
            }

            Debug.WriteLine("Postpone shifting to next frame and return");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UiDispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(16), async () =>
                {
                    try
                    {
                        Debug.WriteLine($"Starting ShiftTimer at {DateTime.Now:HH:mm:ss.fff}");
                        await PerformShiftDownAsync();
                        
                        _shiftDelayCts?.Cancel();

                        _shiftDelayCts = new();

                        var token = _shiftDelayCts.Token;

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(500, token);

                                _isShiftingDown = false;

                                Debug.WriteLine($"ShiftTimer elapsed at {DateTime.Now:HH:mm:ss.fff}");
                            }
                            catch (OperationCanceledException ex)
                            {}
                        });

     
                    }
                    catch (Exception ex)
                    {
                        var mess = ex.Message;
                    }
                    finally
                    {
                        var breakpoint667 = 1;    
                    }
                });
            });

            return Task.CompletedTask;
        }
        #endregion

        
        #region Region Task PerformShiftDownAsync()
        private async Task PerformShiftDownAsync()
        {
           
            Debug.WriteLine("From postponed PerformShiftDownAsync: UI TRY TO SHIFT DOWN");

            int maxItemsToShift = _AllItemsList.Count - UiCollectionSizeInRows - 1;
            int shiftAmount = Math.Clamp(maxItemsToShift, 0, UiScrollThreshold);

            int newUiIndex = ActualTopVisibleItemIndex - shiftAmount;
            if (newUiIndex < 0) newUiIndex = 0;

            
            /*************        HandleUiWindowShiftDown         **************************/

            if (HandleUiWindowShiftDown(shiftAmount))
            { 
                // only needed for debugging
                var testItem = DisplayItemCollection[1];
   
                Debug.WriteLine($"Leaving postponed PerformShiftDownAsync() after shifting at {DateTime.Now:HH:mm:ss.fff}\r\n");
                return;
            }    
        }
        #endregion


        #region Region  Function bool HandleUiWindowShiftDown
        private bool HandleUiWindowShiftDown(int amount)
        {
            List<EntityDisplayItem> DisplayItemList  = new();

            var lastUiWindowStartIndex_local = UiWindowStartIndex;

            UiWindowStartIndex += amount;

            
            if (lastUiWindowStartIndex_local != UiWindowStartIndex)
            {

                SW.Restart();
                string SWStartTime = ($"{SW.ElapsedMilliseconds,8} ms\" ShiftDown gestartet");

                // Copy content of Observable Collection to list (manipulating of list doesn't affekt Ui)

                DisplayItemList = DisplayItemCollection.ToList<EntityDisplayItem>();

                /*
                DisplayItemList.Clear();
                for (int i = 0; i < UiCollectionSizeInRows; i++)
                {
                    DisplayItemList.Add(new EntityDisplayItem(DisplayItemCollection[i].EntityRow));
                }
                */

                // DisplayItemCollection.SuspendNotifications();

                int maxShiftCount = Math.Max(0, DisplayItemCollection.Count - amount);

                var firstVisibleItem = DisplayItemCollection[ActualTopVisibleItemIndex];
                var targetVisibleItem = DisplayItemCollection[ActualTopVisibleItemIndex - amount];

                // Shift items down in List

                for (int i = 0; i < maxShiftCount; i++)
                {
                    DisplayItemList[i].EntityRow = DisplayItemList[i + amount].EntityRow;
                }


                string SWInnerShiftTime = ($"{SW.ElapsedMilliseconds,8} ms\" Items verschoben");





                // fill upper places in List with items from AllItemsList
                for (int i = maxShiftCount; i < DisplayItemCollection.Count; i++)       
                    {
                        int globalIndex = UiWindowStartIndex + i;
                      //  var entity = _entityCache.GetEntityAt(globalIndex);

                        

                        DisplayItemList[i].EntityRow = new EntityDisplayItem(_AllItemsList[globalIndex].EntityRow).EntityRow;

                    Debug.WriteLine($"Counter i = {i}  UIWindwowStartIndex: {UiWindowStartIndex} globalIndex = {globalIndex} Length of _AllItemsList: {_AllItemsList.Count}");

                       // DisplayItemList[i] = ;
                    }
                    


                    // DisplayItemCollection.ResumeNotifications(sendReset: true);

                    // DisplayItemCollection.SuspendNotifications();


                    // DisplayItemCollection.SuspendNotifications();


                    // DisplayItemCollection[50] = DisplayItemList[50];


                    // RoSchmi changed for tests




                    for (int i = 0; i < DisplayItemList.Count; i++)
                {
                    //  DisplayItemCollection[i].EntityRow = DisplayItemList[i].EntityRow;

                    DisplayItemCollection[i] = DisplayItemList[i];

                    //  DisplayItemCollection[i].EntityRow = "AA";

                    // Alternative for new Instance of ObservableCollection (not used)
                    // DisplayItemCollection.Add(DisplayItemList[i]);
                }


                //RequestScrollToIndex?.Invoke(newUiIndex);

                // Set tar
                DisplayItemCollection[ActualTopVisibleItemIndex - amount] = targetVisibleItem;

                RequestScrollToItem?.Invoke(targetVisibleItem);



                //DisplayItemCollection.ResumeNotifications(sendReset: true);




                // DisplayItemCollection.ReplaceAll(DisplayItemList);

                //DisplayItemCollection[i].EntityRow = 



                string SWFillTime = ($"{SW.ElapsedMilliseconds,8} ms\" Items eingesetzt");

                string SWShiftTime = ($"{SW.ElapsedMilliseconds,8} ms\" UiWindow geshiftet");
                Debug.WriteLine($"Down {SWShiftTime}");

                UiWindowStartIndex = Math.Max(0, UiWindowStartIndex);

                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Region InitializeAllItems
        private void InitializeAllItems(int count)
        {
            string exampleString = "The very, very, very, very, very, very, very, very long content of each row";
            _AllItemsList.Clear();
            for (int i = 0; i < count; i++)
            {
                _AllItemsList.Add(new EntityDisplayItem($"{i,5}   {exampleString}"));
            }
        }
        #endregion

        #region Region ReInitialize
        [RelayCommand]
        private void ReInitialize()
        {
            Debug.WriteLine($"Re-initialized ObservableCollection");
            int counter = DisplayItemCollection.Count;
            for (int i = 0; i < counter; i++)
            {
                DisplayItemCollection[i] = new EntityDisplayItem(_AllItemsList[i].EntityRow);   
            }
            UiWindowStartIndex = 0;
            RequestScrollToIndex?.Invoke(0);

            int breakpoint300 = 1;
        }
        #endregion

        #region Region InitializeDisplayItemCollection

        private void InitializeDisplayItemCollection(int count)
        {  
            for (int i = 0; i < count; i++)
            {
                DisplayItemCollection.Add(new EntityDisplayItem(_AllItemsList[i].EntityRow));
            }
            int breakpoint310 = 1;
        }
        #endregion

        #region Region OnDisplayCollectionChanged (is used, function actually not known)
        private void OnDisplayCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"CollectionChangedEvent");
            int breakpoint258 = 1;
        }
        #endregion

        public void VerticallyScrolledCollection(ItemsViewScrolledEventArgs e)
        {
            Debug.WriteLine($"{UiWindowStartIndex}");
        }
    }
}
