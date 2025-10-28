plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.compose)
    id("org.sonarqube")
    id("jacoco")
    id("com.google.gms.google-services")
}

android {
    namespace = "com.exemple.amfootball"
    compileSdk {
        version = release(36)
    }

    defaultConfig {
        applicationId = "com.exemple.amfootball"
        minSdk = 28
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
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
//teste 2
// Configuração da tarefa Jacoco para gerar o relatório XML
tasks.register<JacocoReport>("jacocoTestReport") {
    dependsOn("testDebugUnitTest") // Corre depois dos testes unitários

    reports {
        xml.required.set(true)
        html.required.set(false) //falso porque o sonarqube so precisa do xml
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
    implementation(libs.androidx.material3)
    implementation(libs.androidx.compose.runtime)
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.compose.ui.test.junit4)
    debugImplementation(libs.androidx.compose.ui.tooling)
    debugImplementation(libs.androidx.compose.ui.test.manifest)

    //Dependencias FireBas para Auth
    // Firebase Bill of Materials (BoM) - gere as versões
    implementation(platform("com.google.firebase:firebase-bom:34.4.0"))
    // Dependência do Firebase Authentication
    implementation("com.google.firebase:firebase-auth")
    // Dependência para o Login com Google (necessária)
    implementation("com.google.android.gms:play-services-auth:21.2.0")
    // Dependência para o Login com Facebook (necessária para o Facebook)
    implementation("com.facebook.android:facebook-login:latest.release")

    // 3. (Opcional, mas recomendado) Analytics
    implementation("com.google.firebase:firebase-analytics")

    implementation("androidx.compose.material:material-icons-extended:1.6.7")
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
    // Coil for images
    implementation("io.coil-kt:coil-compose:2.4.0")

    // Bibliotecas para chamadas de rede
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    implementation("com.squareup.okhttp3:okhttp:4.12.0")
}

