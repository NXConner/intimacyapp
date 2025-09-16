-dontobfuscate
-keepattributes *Annotation*
-keep class androidx.** { *; }
-keep class com.google.** { *; }
-keep class kotlinx.** { *; }
-keep class okhttp3.** { *; }
-keep class okio.** { *; }
-keep class org.jetbrains.** { *; }

-keep class androidx.work.** { *; }
-keep class androidx.camera.** { *; }

-dontwarn javax.annotation.**
-dontwarn org.checkerframework.**
