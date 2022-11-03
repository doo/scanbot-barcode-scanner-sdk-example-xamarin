rm -rf ScanbotBarcodeSDKFormsExample.Android/obj
rm -rf ScanbotBarcodeSDKFormsExample.Android/bin

rm -rf ScanbotBarcodeSDKFormsExample.iOS/obj
rm -rf ScanbotBarcodeSDKFormsExample.iOS/bin

msbuild ScanbotBarcodeSDKFormsExample.sln /p:Configuration=Release /t:Clean

dotnet nuget locals all --clear
nuget locals all --clear
rm -rf packages

nuget restore



