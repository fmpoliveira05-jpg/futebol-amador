// Top-level build file where you can add configuration options common to all sub-projects/modules.
plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.android) apply false
    alias(libs.plugins.kotlin.compose) apply false

    id("org.sonarqube") version "7.0.1.6134" apply false
    id("com.google.gms.google-services") version "4.4.4" apply false
    id("org.jetbrains.dokka") version "2.1.0" apply false

    alias(libs.plugins.ksp) apply false
    alias(libs.plugins.hilt.android) apply false
}