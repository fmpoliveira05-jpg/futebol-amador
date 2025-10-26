package com.example.amfootball

import android.Manifest
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import com.example.amfootball.ui.AMFootballApp
import com.example.amfootball.ui.components.NavBar.MainNavigation
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerNavBarHomePage
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerTeam
import com.example.amfootball.ui.screens.HomePageScreen

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        requestLocationPermission.launch(Manifest.permission.ACCESS_FINE_LOCATION)

        enableEdgeToEdge()
        setContent {
            MainNavigation()
            //AMFootballApp()
            //NavigatonDrawerTeam()
        }
    }

    private val requestLocationPermission = registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        // handled inside composables as well; kept here for completeness
    }
}