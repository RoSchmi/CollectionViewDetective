using CollectionViewDetective.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using CollectionViewDetective.Extensions;

namespace CollectionViewDetective.ViewModels
{
    public partial class DisplayViewModel : ObservableObject
    {
        public Action<int>? RequestScrollToIndex { get; set; }

        public Action<EntityDisplayItem>? RequestScrollToItem { get; set; }

        public Action<bool>? RequestSuppressLayout { get; set; }

        [ObservableProperty]
        private int uiCollectionSizeInRows = 100;            // Größe der DisplayItemCollection

        private int _UiCollectionSizeInRowsOriginal = 100;  // Variable to hold the original value 


        [ObservableProperty]
        private int uiScrollThreshold = 20;    // Schwelle für Fenster-Verschiebung

        private int _UiScrollThresholdOriginal = 20;  //Variable to hold the original value

        [ObservableProperty]
        private string neededTimeAsString;

        private static readonly Stopwatch SW = Stopwatch.StartNew();

        
       // public int UiWindowStartIndex { get; private set; } = 0;
        public int UiWindowEndIndex => UiWindowStartIndex + uiCollectionSizeInRows - 1;

        [ObservableProperty]
        private int uiWindowStartIndex;

        [ObservableProperty]
        private int actualTopVisibleItemIndex;

        [ObservableProperty]
        private int actualBottomVisibleItemIndex;

        //public int DisplayCollectionWidth { get; set; } = 1000;

        [ObservableProperty]
        private double displayCollectionWidth = 1000;

        [ObservableProperty]
        private bool showSettings = false;

        [ObservableProperty]

        private ItemsUpdatingScrollMode? updatingScrollMode;

        List <EntityDisplayItem> _AllItemsList = new List<EntityDisplayItem>();

        public ObservableCollection<EntityDisplayItem> DisplayItemCollection { get; set; } = new ObservableCollection<EntityDisplayItem>();

        //[ObservableProperty]
        //private ObservableCollectionExtended<EntityDisplayItem> displayItemCollection = new();

        [ObservableProperty]
        private string buttonText = "Re-Initialize Tables";

        public IDispatcher UiDispatcher { get; set; }

        private CancellationTokenSource? _shiftDelayCts;

        private bool _isShiftingDown = false;

        private DateTime _startShiftTime = DateTime.Now;

        

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
                        _startShiftTime = DateTime.Now;
                        Debug.WriteLine($"Starting ShiftTimer at {_startShiftTime:HH:mm:ss.fff}");
                        
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

                Debug.WriteLine($"Before Delay {DateTime.Now:HH:mm:ss.fff}");
                await Task.Delay(1);

                DateTime afterTime = DateTime.Now;
                Debug.WriteLine($"After Delay {afterTime:HH:mm:ss.fff}");


                TimeSpan neededTimeToShift = DateTime.Now - _startShiftTime;
                NeededTimeAsString = $"{neededTimeToShift.Seconds}.{neededTimeToShift:fff}";
                Debug.WriteLine($"Leaving postponed PerformShiftDownAsync() after shifting at {DateTime.Now:HH:mm:ss.fff}, needed Time (sec.ms: {NeededTimeAsString}  \r\n");      
                return;
            }    
        }
        #endregion

        #region Region  Function bool HandleUiWindowShiftDown
        private bool HandleUiWindowShiftDown(int amount)
        {
            var originalUiWindowStartIndex = UiWindowStartIndex;

            UiWindowStartIndex += amount;

            if (originalUiWindowStartIndex != UiWindowStartIndex)
            {

                SW.Restart();
                string SWStartTime = ($"{SW.ElapsedMilliseconds,8} ms\" ShiftDown gestartet");
  
                // Set UpdatingScrollMode in .xaml
                UpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

                int startPointer = DisplayItemCollection.Count;

                RequestSuppressLayout?.Invoke(true);

                try
                {
                    // Add new items to ObservableCollection at bottom
                    for (int i = 0; i < amount; i++)
                    {
                        int globalIndex = startPointer + originalUiWindowStartIndex + i;
                        DisplayItemCollection.Add(new EntityDisplayItem(_AllItemsList[globalIndex].EntityRow));
                    }

                    string SWInnerShiftTime = ($"{SW.ElapsedMilliseconds,8} ms\" Items added at bottom");

                    Debug.WriteLine($"{SWInnerShiftTime}");

                    Debug.WriteLine($"Starting to remove items from top at {DateTime.Now:HH:mm:ss.fff}");

                    while (DisplayItemCollection.Count > UiCollectionSizeInRows)
                    {
                        DisplayItemCollection.RemoveAt(0);
                    }
                }
                finally
                {
                    RequestSuppressLayout?.Invoke(false);
                }

                Debug.WriteLine($"Finished to remove items from top at {DateTime.Now:HH:mm:ss.fff}");

                string SWShiftTime = ($"{SW.ElapsedMilliseconds,8} ms\" UiWindow shiftet completely");
                Debug.WriteLine($"{SWShiftTime}");

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
            //int counter = DisplayItemCollection.Count;
            int counter = UiCollectionSizeInRows;
            DisplayItemCollection.Clear();
            for (int i = 0; i < counter; i++)
            {
                DisplayItemCollection.Add(new EntityDisplayItem(_AllItemsList[i].EntityRow));
                //DisplayItemCollection[i] = new EntityDisplayItem(_AllItemsList[i].EntityRow);   
            }
            UiWindowStartIndex = 0;
            RequestScrollToIndex?.Invoke(0);

            int breakpoint300 = 1;
        }
        #endregion

        #region Region Change Settings
        [RelayCommand]

        private void OpenSettings()
        {
            _UiCollectionSizeInRowsOriginal = UiCollectionSizeInRows;
            _UiScrollThresholdOriginal = UiScrollThreshold;
            ShowSettings = true;

        }
        [RelayCommand]
        private void CloseSettings()
        {
            ReInitialize();
            ShowSettings = false;
        }
        [RelayCommand]
        private void CloseSettingsCancel()
        {
            UiCollectionSizeInRows = _UiCollectionSizeInRowsOriginal;
            UiScrollThreshold = _UiScrollThresholdOriginal;
            ShowSettings = false;
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
        }
        #endregion

        public void VerticallyScrolledCollection(ItemsViewScrolledEventArgs e)
        {
            Debug.WriteLine($"UiWindowStartIndex = {UiWindowStartIndex}");
        }
    }
}
