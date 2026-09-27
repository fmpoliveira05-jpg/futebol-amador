# Regras do R8 para a build de release (isMinifyEnabled + isShrinkResources).
#
# As bibliotecas com regras próprias (Room, Hilt/Dagger, Firebase, OkHttp, Coil, CameraX, Compose)
# não precisam de nada aqui. As regras abaixo cobrem o que usa reflexão: os DTOs lidos e escritos
# pelo Gson (Retrofit, SignalR, Room e a cache da classificação), os documentos do Firestore e as
# interfaces do Retrofit com funções suspend.
# NOTA: não verificado neste ambiente (sem Android SDK) — compilar a release e testar login, listas,
# chat e hubs antes de publicar.

# Números de linha nos stack traces (o nome do ficheiro fica escondido).
-keepattributes SourceFile,LineNumberTable
-renamesourcefileattribute SourceFile

# Genéricos e anotações (Gson, Retrofit, Firestore).
-keepattributes Signature,InnerClasses,EnclosingMethod,*Annotation*,RuntimeVisibleAnnotations,RuntimeVisibleParameterAnnotations,AnnotationDefault

# ---------- Modelos da aplicação ----------
# O Gson e o Firestore leem e escrevem os campos pelo nome e criam os objetos por reflexão:
# mantêm-se os campos e os construtores dos modelos (as classes de UI continuam a ser reduzidas).
-keep class com.example.amfootball.data.** { <fields>; <init>(...); }
-keep class com.example.amfootball.competicao.** { <fields>; <init>(...); }
-keep class com.example.amfootball.domains.** { <fields>; <init>(...); }
-keepclassmembers enum com.example.amfootball.** {
    <fields>;
    public static **[] values();
    public static ** valueOf(java.lang.String);
}

# ---------- Gson ----------
-keep class com.google.gson.reflect.TypeToken { *; }
-keep class * extends com.google.gson.reflect.TypeToken
-keepclassmembers,allowobfuscation class * {
    @com.google.gson.annotations.SerializedName <fields>;
}
-keep class * implements com.google.gson.TypeAdapterFactory
-keep class * implements com.google.gson.JsonSerializer
-keep class * implements com.google.gson.JsonDeserializer
-dontwarn sun.misc.**

# ---------- Retrofit (funções suspend e anotações nas interfaces) ----------
-keepclassmembers,allowshrinking,allowobfuscation interface * {
    @retrofit2.http.* <methods>;
}
-keep,allowobfuscation,allowshrinking interface retrofit2.Call
-keep,allowobfuscation,allowshrinking class retrofit2.Response
-keep,allowobfuscation,allowshrinking class kotlin.coroutines.Continuation
-dontwarn retrofit2.**
-dontwarn javax.annotation.**
-dontwarn kotlin.Unit
-dontwarn org.codehaus.mojo.animal_sniffer.*

# ---------- Moshi (conversor registado no Retrofit) ----------
-dontwarn com.squareup.moshi.**

# ---------- OkHttp ----------
-dontwarn okhttp3.internal.platform.**
-dontwarn org.conscrypt.**
-dontwarn org.bouncycastle.**
-dontwarn org.openjsse.**

# ---------- SignalR (Java) e RxJava ----------
-keep class com.microsoft.signalr.** { *; }
-dontwarn com.microsoft.signalr.**
-dontwarn io.reactivex.rxjava3.**
-dontwarn org.slf4j.**

# ---------- Cloudinary, osmdroid e libphonenumber ----------
-keep class com.cloudinary.** { *; }
-dontwarn com.cloudinary.**
-dontwarn org.osmdroid.**
-keep class com.google.i18n.phonenumbers.** { *; }
