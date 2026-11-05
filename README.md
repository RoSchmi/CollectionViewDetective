### CollectionViewDetective

Explore and monitor behavior when scrolling down using Maui ObservableCollection and CollectionView nested in a horizontal ScrollView.

Works for Windows, iOS and Android

This Repo was made because there were difficulties to achieve smoothly scrolling on Android.

In the master branch you see the version, when the problem was solved by setting: 

_recyclerView.HasFixedSize = true;

This solution works especially in conditions where the size of the bound ObservableCollection is held constant.

In the branch original you see the original version with stuttering scrolling on Android





