#!/bin/bash
# Quick deploy script for Nostr Connect Android app

export ANDROID_HOME=/home/mike/Android/Sdk
export PATH=$PATH:$ANDROID_HOME/platform-tools

echo "🔨 Building Android app..."
dotnet build NostrConnect.Maui/NostrConnect.Maui.csproj -f net9.0-android -c Debug

if [ $? -eq 0 ]; then
    echo "📦 Installing to device..."
    adb install -r NostrConnect.Maui/bin/Debug/net9.0-android/com.companyname.nostrconnect.maui-Signed.apk
    
    if [ $? -eq 0 ]; then
        echo "🧹 Clearing logcat buffer..."
        adb logcat -c
        
        echo "🚀 Launching app..."
        adb shell am start -n com.companyname.nostrconnect.maui/crc64bcb87718c7ec06ab.MainActivity
        
        echo "✅ App deployed and launched!"
        echo "📱 Streaming app-specific debug logs (Press Ctrl+C to stop)..."
        echo "----------------------------------------"
        
        # Give the app a moment to fully launch
        sleep 2
        
        # Stream logs filtered to only show the MAUI app using grep
        adb logcat -v time | grep --line-buffered "strconnect.maui"
    else
        echo "❌ Installation failed"
    fi
else
    echo "❌ Build failed"
fi
