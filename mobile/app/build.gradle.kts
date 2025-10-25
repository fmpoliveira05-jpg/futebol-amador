plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.compose)
    id("org.sonarqube") version "4.4.1.3373" // Adicione esta linha
    id("jacoco") // Adicione esta linha (para os relatórios de cobertura)
}

android {
    namespace = "com.example.amfootball"
    compileSdk {
        version = release(36)
    }

    defaultConfig {
        applicationId = "com.example.amfootball"
        minSdk = 28
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    kotlinOptions {
        jvmTarget = "11"
    }
    buildFeatures {
        compose = true
    }
}

tasks.withType<Test> {
    reports.junitXml.required.set(true)
}

sonarqube {
    properties {
        property("sonar.projectKey", "LDS-Frontend-Mobile")

        property("sonar.junit.reportsPath", "build/test-results/testDebugUnitTest")
        property("sonar.coverage.jacoco.xmlReportPath", "build/reports/jacoco/jacocoTestReport/jacocoTestReport.xml")

        property("sonar.sources", "src/main/kotlin")
        property("sonar.tests", "src/test/kotlin")
    }
}

dependencies {
    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.activity.compose)
    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.compose.ui)
    implementation(libs.androidx.compose.ui.graphics)
    implementation(libs.androidx.compose.ui.tooling.preview)
    implementation(libs.androidx.compose.material3)
    implementation(libs.play.services.location)
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.compose.ui.test.junit4)
    debugImplementation(libs.androidx.compose.ui.tooling)
    debugImplementation(libs.androidx.compose.ui.test.manifest)

    // Navigation
    implementation("androidx.navigation:navigation-compose:2.7.0")
// ViewModel + Compose
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.6.2")
// Coroutines
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")
// Google Maps Compose & Play Services
    implementation("com.google.android.gms:play-services-maps:18.1.0")
    implementation("com.google.android.gms:play-services-location:21.1.0")
    implementation("com.google.maps.android:maps-compose:2.12.0")
// Optional: Coil for images
    implementation("io.coil-kt:coil-compose:2.4.0")
}

