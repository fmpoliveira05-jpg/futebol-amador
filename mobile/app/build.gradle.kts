plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.compose)

    id("org.sonarqube")
    id("jacoco")
    id("com.google.gms.google-services")
    id("org.jetbrains.dokka")

    alias(libs.plugins.ksp)
    alias(libs.plugins.hilt.android)
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

        //testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        //Runner do Dagger-Hilt
        testInstrumentationRunner = "com.example.amfootball.mockWebServer.CustomTestRunner"
    }

    buildTypes {
        getByName("debug") {
            // Habilita a cobertura de testes para a build de debug
            enableUnitTestCoverage = true
            enableAndroidTestCoverage = true // Para quando tivermos testes da UI
        }
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
// Configuração da tarefa Jacoco para gerar o relatório XML
tasks.register<JacocoReport>("jacocoTestReport") {
    dependsOn("testDebugUnitTest")

    reports {
        xml.required.set(true)
        html.required.set(false)
    }

    classDirectories.setFrom(
        layout.buildDirectory.map { buildDir ->
            buildDir.dir("tmp/kotlin-classes/debug").asFileTree.matching {
                exclude("**/R.class", "**/R\$*.class", "**/BuildConfig.*", "**/Manifest*.*")
            }
        }
    )

    // Define onde o Jacoco procura os ficheiros de código-fonte
    sourceDirectories.setFrom(files("$projectDir/src/main/kotlin"))

    // Define onde o Jacoco procura os resultados da execução dos testes
    executionData.setFrom(layout.buildDirectory.map { it.file("jacoco/testDebugUnitTest.exec") })
}

tasks.withType<Test> {
    reports.junitXml.required.set(true)
}

sonarqube {
    properties {
        property("sonar.projectKey", "LDS-Frontend-Mobile")

        property("sonar.junit.reportsPath", "build/test-results/testDebugUnitTest")
        property(
            "sonar.coverage.jacoco.xmlReportPath",
            "build/reports/jacoco/jacocoTestReport/jacocoTestReport.xml"
        )

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
    implementation(libs.androidx.material3)
    implementation(libs.androidx.compose.runtime)
    implementation(libs.androidx.compose.foundation)
    implementation(libs.androidx.compose.foundation.layout)
    implementation(libs.androidx.room.ktx)
    implementation(libs.androidx.compose.runtime.livedata)
    implementation(libs.androidx.lifecycle.viewmodel.ktx)
    implementation(libs.androidx.ui)
    implementation(libs.androidx.appcompat)
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.compose.ui.test.junit4)
    debugImplementation(libs.androidx.compose.ui.tooling)
    debugImplementation(libs.androidx.compose.ui.test.manifest)

    //Dependencia para ir buscar as dependências dos prefixos de numero telefonico
    implementation("com.googlecode.libphonenumber:libphonenumber:8.13.27")

    //Dependencias FireBas para Auth
    implementation(platform("com.google.firebase:firebase-bom:34.4.0"))

    // Dependência do Firebase Authentication
    implementation("com.google.firebase:firebase-auth:24.0.1")
    implementation("com.google.firebase:firebase-firestore")

    //Push Notification FireBase
    implementation("com.google.firebase:firebase-messaging")

    implementation("androidx.compose.material:material-icons-extended:1.6.7")

    implementation("androidx.navigation:navigation-compose:2.7.0")
    // ViewModel e Compose
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.6.2")

    // Coroutines
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3")

    // Google Maps Compose & Play Services
    implementation("com.google.android.gms:play-services-maps:18.1.0")
    implementation("com.google.maps.android:maps-compose:2.12.0")

    // Coil for images
    implementation("io.coil-kt:coil-compose:2.6.0")

    // Bibliotecas para chamadas de rede
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    implementation("com.squareup.retrofit2:converter-moshi:2.9.0")

    implementation("com.squareup.okhttp3:okhttp:4.12.0")
    implementation("com.squareup.okhttp3:logging-interceptor:4.12.0")

    //Dagger.Hilt
    implementation("com.google.dagger:hilt-android:2.51.1")
    ksp("com.google.dagger:hilt-compiler:2.51.1") // Para KSP

    // A biblioteca para usar hiltViewModel() no Compose
    implementation("androidx.hilt:hilt-navigation-compose:1.2.0")

    //Espresso Ui Testing
    androidTestImplementation("androidx.test.espresso:espresso-core:3.5.1")

    // Hilt Testing
    androidTestImplementation("com.google.dagger:hilt-android-testing:2.57.1")
    kspAndroidTest("com.google.dagger:hilt-android-compiler:2.57.1")
    androidTestImplementation("androidx.test:rules:1.6.1")
    // MockWebServer
    androidTestImplementation("com.squareup.okhttp3:mockwebserver:4.11.0")

    // Mockito para Testes Android (Instrumentados)
    androidTestImplementation("org.mockito:mockito-android:5.7.0")
    androidTestImplementation("org.mockito:mockito-core:5.7.0")

    //Signal R
    implementation("com.microsoft.signalr:signalr:7.0.0")
}