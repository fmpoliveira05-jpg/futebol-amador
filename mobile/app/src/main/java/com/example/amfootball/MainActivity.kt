package com.example.amfootball

import android.Manifest
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.material.MaterialTheme
import androidx.compose.material.Surface
import androidx.compose.runtime.*
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.amfootball.ui.AMFootballApp
import com.example.amfootball.ui.theme.AMFootballTheme

class MainActivity : ComponentActivity() {

    private val requestLocationPermission = registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        // handled inside composables as well; kept here for completeness
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // ask permission once on launch (also handled in composables)
        requestLocationPermission.launch(Manifest.permission.ACCESS_FINE_LOCATION)

        setContent {
            AMFootballTheme {
                Surface(color = MaterialTheme.colors.background) {
                    AMFootballApp()
                }
            }
        }
    }
}